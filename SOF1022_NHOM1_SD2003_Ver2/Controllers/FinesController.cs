using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Services;

[SessionAuthorize]
public class FinesController : Controller
{
    private readonly LibraryDbContext _context;
    private readonly INotificationService _notificationService;

    public FinesController(LibraryDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // GET: Fines
    [PermissionAuthorize("fine.view")]
    public async Task<IActionResult> Index()
    {
        var fines = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Include(f => f.FineDetails)
                .ThenInclude(fd => fd.Book)
            .Include(f => f.Books)
            .OrderByDescending(f => f.Id)
            .ToListAsync();

        return View(fines);
    }

    // GET: Fines/Pay/5
    [PermissionAuthorize("fine.process_payment")]
    public async Task<IActionResult> Pay(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fine = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Include(f => f.Books)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null)
        {
            return NotFound();
        }

        return View(fine);
    }

    // POST: Fines/Pay/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("fine.process_payment")]
    public async Task<IActionResult> Pay(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.BorrowSlips)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fine == null)
        {
            return NotFound();
        }

        fine.IsPaid = true;
        fine.PaidDate = DateTime.Now;
        fine.Status = "Đã thanh toán";

        _context.Update(fine);
        await _context.SaveChangesAsync();

        if (fine.BorrowSlips != null)
        {
            await _notificationService.NotifyUserAsync(
                fine.BorrowSlips.ReaderId,
                "Reader",
                NotificationType.FinePaid,
                "Đã thanh toán khoản phạt",
                $"Bạn đã thanh toán khoản phạt {fine.Amount:N0} VNĐ thành công.",
                link: "/Fines/MyFines",
                createdBy: "Người dùng");
        }

        TempData["Success"] = "Đã thanh toán tiền phạt thành công.";

        return RedirectToAction(nameof(Index));
    }

    // GET: FINESS/Details/5
    [PermissionAuthorize("fine.view")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fine = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Include(f => f.FineDetails)
                .ThenInclude(fd => fd.Book)
            .Include(f => f.BorrowDetails)
                .ThenInclude(d => d.Books)
            .Include(f => f.Books)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (fine == null)
        {
            return NotFound();
        }

        return View(fine);
    }

    // GET: Fines/PrintReceipt/5
    [PermissionAuthorize("fine.view")]
    public async Task<IActionResult> PrintReceipt(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fine = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Include(f => f.FineDetails)
                .ThenInclude(fd => fd.Book)
            .Include(f => f.Books)
            .Include(f => f.BorrowDetails)
                .ThenInclude(d => d.Books)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (fine == null)
        {
            return NotFound();
        }

        return View(fine);
    }

    [PermissionAuthorize("fine.create")]
    public async Task<IActionResult> GenerateFine(int borrowSlipId)
    {
        var slip = await _context.BorrowSlips
            .Include(x => x.Readers)
            .FirstOrDefaultAsync(x => x.Id == borrowSlipId);

        if (slip == null)
        {
            TempData["Error"] = "Không tìm thấy phiếu mượn.";
            return RedirectToAction("Index", "BorrowSlips");
        }

        if (slip.DueDate >= DateTime.Now)
        {
            TempData["Error"] = "Phiếu chưa quá hạn.";
            return RedirectToAction("Details", "BorrowSlips", new { id = borrowSlipId });
        }

        int lateDays = (DateTime.Now - slip.DueDate).Days;

        decimal amount = lateDays * 5000;

        var existing = await _context.Fines
            .FirstOrDefaultAsync(x => x.BorrowSlipId == borrowSlipId);

        if (existing != null)
        {
            TempData["Error"] = "Phiếu đã có tiền phạt.";
            return RedirectToAction("Details", "BorrowSlips", new { id = borrowSlipId });
        }

        var fine = new Fines
        {
            BorrowSlipId = borrowSlipId,
            Amount = amount,
            CreatedDate = DateTime.Now
        };

        _context.Fines.Add(fine);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã tạo tiền phạt.";

        return RedirectToAction("Index");
    }

    [PermissionAuthorize("fine.process_payment")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var fine = await _context.Fines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (fine == null)
        {
            return NotFound();
        }

        fine.IsPaid = true;

        _context.Update(fine);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã thanh toán tiền phạt.";

        return RedirectToAction(nameof(Index));
    }

    [PermissionAuthorize("fine.view")]
    public async Task<IActionResult> Unpaid()
    {
        var fines = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Where(f => !f.IsPaid)
            .ToListAsync();

        return View(fines);
    }

    public async Task<IActionResult> History()
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int currentReaderId = int.Parse(userIdString);

        var fines = await _context.Fines
            .Include(f => f.Books)
            .Include(f => f.BorrowDetails)
                .ThenInclude(d => d.Books)
            .Where(f => f.ReaderId == currentReaderId && f.IsPaid)
            .OrderByDescending(f => f.PaidDate)
            .ToListAsync();

        return View(fines);
    }

    // GET: FINESS/Create
    [PermissionAuthorize("fine.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: FINESS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("fine.create")]
    public async Task<IActionResult> Create([Bind("Id,BorrowSlipId,Amount,Reason,CreatedDate,PaidDate,FinesType,IsPaid,BorrowSlips")] Fines fines)
    {
        if (ModelState.IsValid)
        {
            _context.Add(fines);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(fines);
    }

    // GET: FINESS/Edit/5
    [PermissionAuthorize("fine.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fines = await _context.Fines.FindAsync(id);
        if (fines == null)
        {
            return NotFound();
        }
        return View(fines);
    }

    // POST: FINESS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("fine.edit")]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,BorrowSlipId,Amount,Reason,CreatedDate,PaidDate,FinesType,IsPaid,BorrowSlips")] Fines fines)
    {
        if (id != fines.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(fines);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FinesExists(fines.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(fines);
    }

    // GET: FINESS/Delete/5
    [PermissionAuthorize("fine.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fines = await _context.Fines
            .FirstOrDefaultAsync(m => m.Id == id);
        if (fines == null)
        {
            return NotFound();
        }

        return View(fines);
    }

    // POST: FINESS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("fine.delete")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var fines = await _context.Fines.FindAsync(id);
        if (fines != null)
        {
            _context.Fines.Remove(fines);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FinesExists(int? id)
    {
        return _context.Fines.Any(e => e.Id == id);
    }

    // ==========================
    // Tiền phạt
    // ==========================
    public async Task<IActionResult> MyFines()
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int currentReaderId = int.Parse(userIdString);

        var myFines = await _context.Fines
            .IgnoreQueryFilters()
            .Include(f => f.Books)
            .Include(f => f.BorrowDetails)
                .ThenInclude(d => d.Books)
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b.BorrowDetails)
                    .ThenInclude(d => d.Books)
            .Include(f => f.FineDetails!)
                .ThenInclude(fd => fd.Book)
            .Where(f => f.ReaderId == currentReaderId)
            .OrderByDescending(f => f.CreatedDate)
            .ToListAsync();

        foreach (var fine in myFines)
        {
            if (fine.Books == null && fine.BookId.HasValue)
            {
                fine.Books = await _context.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == fine.BookId.Value);
            }
        }
 
        return View(myFines);
    }

    // GET: Fines/PayFine/5
    [HttpGet]
    public async Task<IActionResult> PayFine(int id)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int currentReaderId = int.Parse(userIdString);

        var fine = await _context.Fines
            .Include(f => f.Books)
            .Include(f => f.BorrowDetails)
                .ThenInclude(d => d.Books)
            .FirstOrDefaultAsync(f => f.Id == id && f.ReaderId == currentReaderId);

        if (fine != null && fine.Books == null && fine.BookId.HasValue)
        {
            fine.Books = await _context.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == fine.BookId.Value);
        }

        if (fine == null)
        {
            TempData["Error"] = "Không tìm thấy khoản phạt hoặc bạn không có quyền truy cập.";
            return RedirectToAction(nameof(MyFines));
        }

        if (fine.IsPaid)
        {
            TempData["Error"] = "Khoản phạt này đã được thanh toán.";
            return RedirectToAction(nameof(MyFines));
        }

        return View(fine);
    }

    // POST: Fines/PayFine/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("PayFine")]
    public async Task<IActionResult> PayFineConfirmed(int id)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int currentReaderId = int.Parse(userIdString);

        var fine = await _context.Fines
            .FirstOrDefaultAsync(f => f.Id == id && f.ReaderId == currentReaderId);

        if (fine != null && fine.Books == null && fine.BookId.HasValue)
        {
            fine.Books = await _context.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == fine.BookId.Value);
        }

        if (fine == null)
        {
            TempData["Error"] = "Không tìm thấy khoản phạt hoặc bạn không có quyền truy cập.";
            return RedirectToAction(nameof(MyFines));
        }

        if (fine.IsPaid)
        {
            TempData["Error"] = "Khoản phạt này đã được thanh toán.";
            return RedirectToAction(nameof(MyFines));
        }

        fine.IsPaid = true;
        fine.PaidDate = DateTime.Now;
        fine.Status = "Đã thanh toán";

        _context.Update(fine);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã thanh toán tiền phạt thành công.";

        return RedirectToAction(nameof(MyFines));
    }

    // POST: Fines/RequestPaymentApproval
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPaymentApproval([FromBody] dynamic payload)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập" });
        }

        int currentReaderId = int.Parse(userIdString);
        int fineId = payload.id;

        var fine = await _context.Fines
            .FirstOrDefaultAsync(f => f.Id == fineId && f.ReaderId == currentReaderId);

        if (fine == null)
        {
            return Json(new { success = false, message = "Không tìm thấy khoản phạt" });
        }

        if (fine.IsPaid)
        {
            return Json(new { success = false, message = "Khoản phạt này đã được thanh toán" });
        }

        // Update status to pending approval instead of directly paid
        fine.Status = "PendingApproval";
        
        _context.Update(fine);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã gửi yêu cầu xác nhận thanh toán" });
    }

    // POST: Fines/ProcessVNPAYPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessVNPAYPayment([FromBody] dynamic payload)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập" });
        }

        int currentReaderId = int.Parse(userIdString);
        int fineId = payload.fineId;
        decimal amount = payload.amount;

        var fine = await _context.Fines
            .FirstOrDefaultAsync(f => f.Id == fineId && f.ReaderId == currentReaderId);

        if (fine == null)
        {
            return Json(new { success = false, message = "Không tìm thấy khoản phạt" });
        }

        if (fine.IsPaid)
        {
            return Json(new { success = false, message = "Khoản phạt này đã được thanh toán" });
        }

        // Mock VNPAY payment processing delay
        await Task.Delay(1500); // Simulate network delay

        // Update fine status to Paid (VNPAY auto-approval)
        fine.IsPaid = true;
        fine.PaidDate = DateTime.Now;
        fine.Status = "Đã thanh toán";
        
        _context.Update(fine);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Thanh toán VNPAY thành công" });
    }

    // POST: Fines/ProcessQRPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessQRPayment([FromBody] dynamic payload)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập" });
        }

        int currentReaderId = int.Parse(userIdString);
        int fineId = payload.fineId;

        var fine = await _context.Fines
            .FirstOrDefaultAsync(f => f.Id == fineId && f.ReaderId == currentReaderId);

        if (fine == null)
        {
            return Json(new { success = false, message = "Không tìm thấy khoản phạt" });
        }

        if (fine.IsPaid)
        {
            return Json(new { success = false, message = "Khoản phạt này đã được thanh toán" });
        }

        // Simulate QR payment processing - auto-approve payment
        fine.IsPaid = true;
        fine.PaidDate = DateTime.Now;
        fine.Status = "Đã thanh toán";

        _context.Update(fine);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Thanh toán QR thành công" });
    }

    // POST: Fines/CheckPaymentStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckPaymentStatus([FromBody] dynamic payload)
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập" });
        }

        int currentReaderId = int.Parse(userIdString);
        int fineId = payload.fineId;

        var fine = await _context.Fines
            .FirstOrDefaultAsync(f => f.Id == fineId && f.ReaderId == currentReaderId);

        if (fine == null)
        {
            return Json(new { success = false, message = "Không tìm thấy khoản phạt" });
        }

        if (fine.IsPaid)
        {
            return Json(new { success = true, paid = true, message = "Đã thanh toán" });
        }

        // Simulate bank transaction checking
        // In a real system, this would integrate with bank APIs to check for transactions
        // containing the transaction code and matching amount
        var paymentReceived = await SimulateBankTransactionCheck(fine.TransactionCode, fine.Amount);

        if (paymentReceived == "success")
        {
            // Update fine status to paid
            fine.IsPaid = true;
            fine.PaidDate = DateTime.Now;
            fine.Status = "Đã thanh toán";

            _context.Update(fine);
            await _context.SaveChangesAsync();

            return Json(new { success = true, paid = true, message = "Thanh toán thành công" });
        }
        else if (paymentReceived == "insufficient")
        {
            return Json(new { success = true, paid = false, insufficient = true, message = "Không đủ tiền" });
        }
        else
        {
            // Still waiting for payment
            return Json(new { success = true, paid = false, insufficient = false, message = "Đang chờ thanh toán" });
        }
    }

    private async Task<string> SimulateBankTransactionCheck(string? transactionCode, decimal expectedAmount)
    {
        // Simulate bank transaction checking logic
        // In production, this would call actual bank APIs to verify transactions

        // For demonstration, randomly return different states to simulate real scenarios
        var random = new Random();
        var randomValue = random.Next(1, 100);

        // 10% chance of immediate success (for testing)
        if (randomValue <= 10)
        {
            return "success";
        }
        // 5% chance of insufficient funds (for testing)
        else if (randomValue <= 15)
        {
            return "insufficient";
        }
        else
        {
            return "waiting";
        }
    }

    [HttpGet]
    public async Task<IActionResult> PrintReceipt(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.BorrowSlips)
                .ThenInclude(b => b!.Readers)
            .Include(f => f.FineDetails)
                .ThenInclude(fd => fd.Book)
            .Include(f => f.Books)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null)
        {
            return NotFound();
        }

        var userRole = HttpContext.Session.GetString("UserRole");
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (userRole == "Reader" && fine.BorrowSlips?.ReaderId != currentUserId && fine.ReaderId != currentUserId)
        {
            return Forbid();
        }

        return View(fine);
    }
}

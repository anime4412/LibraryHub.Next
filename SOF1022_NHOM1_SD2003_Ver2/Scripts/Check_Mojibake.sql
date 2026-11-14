SELECT Id, Title, Keywords FROM Books WHERE Title LIKE '%Ã%' OR Title LIKE '%»%' OR Keywords LIKE '%Ã%';
SELECT Id, FullName, Address FROM Readers WHERE FullName LIKE '%Ã%' OR FullName LIKE '%»%' OR Address LIKE '%Ã%';
SELECT Id, FullName, Address FROM Employees WHERE FullName LIKE '%Ã%' OR FullName LIKE '%»%' OR Address LIKE '%Ã%';
SELECT Id, Name, Biography FROM Authors WHERE Name LIKE '%Ã%' OR Name LIKE '%»%' OR Biography LIKE '%Ã%';
SELECT Id, Name, Description FROM Categories WHERE Name LIKE '%Ã%' OR Name LIKE '%»%' OR Description LIKE '%Ã%';

namespace SOF1022_NHOM1_SD2003_Ver2.Helpers
{
    public static class AgeHelper
    {
        /// <summary>
        /// Calculate the exact age of a person based on their date of birth
        /// </summary>
        /// <param name="dateOfBirth">The person's date of birth</param>
        /// <returns>The exact age in years</returns>
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            
            // Adjust age if birthday hasn't occurred yet this year
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age;
        }

        /// <summary>
        /// Check if a reader meets the minimum age requirement for a book
        /// </summary>
        /// <param name="readerDateOfBirth">The reader's date of birth</param>
        /// <param name="minimumAge">The minimum age required for the book</param>
        /// <returns>True if the reader meets the age requirement, false otherwise</returns>
        public static bool MeetsAgeRequirement(DateTime readerDateOfBirth, int minimumAge)
        {
            var readerAge = CalculateAge(readerDateOfBirth);
            return readerAge >= minimumAge;
        }
    }
}

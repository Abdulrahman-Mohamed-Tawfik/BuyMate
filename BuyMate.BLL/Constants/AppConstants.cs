namespace BuyMate.BLL.Constants
{
    public static class AppConstants
    {
        public static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public const long MaxImageFileSizeBytes = 20 * 1024 * 1024; // 20 MB
        public const string DefaultProfileImageFileName = "Default.webp";
        public const string UserProfileImagesFolder = "UserProfileImages";
        public const string CategoriesFolderName = "Categories";
        public const string ProductsFolderName = "Products";


    }
}

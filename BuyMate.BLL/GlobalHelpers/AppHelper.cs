using System.IO;
using BuyMate.DTO.Enum;

namespace BuyMate.BLL.GlobalHelpers
{
    // Central helper for image path operations (now using enum names directly)
    public static class AppHelper
    {
        public static string GetImagePhysicalPath(ImageTypes type, string fileName)
        {
            return Path.Combine(GetImagesRoot(type), fileName);
        }

        public static string GetImageRelativePath(ImageTypes type, string fileName)
        {
            return $"images/{FolderName(type)}/{fileName}".Replace("\\", "/");
        }

        private static string FolderName(ImageTypes type) => type.ToString();

        private static string GetImagesRoot(ImageTypes type)
        {
            var folder = FolderName(type);
            var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            return root;
        }
    }
}

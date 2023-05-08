using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using MvcCubosGACH.Filters;
using MvcCubosGACH.Models;
using MvcCubosGACH.Services;

namespace MvcCubosGACH.Controllers {
    public class UserController : Controller {

        private ServiceCubos service;
        private ServiceStorageBlobs serviceBlob;
        private string containerName;

        public UserController(ServiceCubos service, ServiceStorageBlobs serviceBlob) {
            this.service = service;
            this.serviceBlob = serviceBlob;
            this.containerName = "userprivate";
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Perfil() {
            string blobName = HttpContext.User.FindFirst("IMAGEN").Value;
            if (blobName != null) {
                BlobContainerClient blobContainerClient = await this.serviceBlob.GetContainerAsync(this.containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                    BlobContainerName = this.containerName,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTime.UtcNow.AddHours(1),
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var uri = blobClient.GenerateSasUri(sasBuilder);
                ViewData["URI"] = uri;
            }

            return View();
        }

        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario, IFormFile file) {
            string extension = System.IO.Path.GetExtension(file.FileName);

            int newId = await this.service.RegisterUser(usuario.Email, usuario.Password, extension, usuario.Nombre);
            string blobName = "user_pic_" + newId + extension;
            
            using (Stream stream = file.OpenReadStream()) {
                await this.serviceBlob.UploadBlobAsync(this.containerName, blobName, stream);
            }
            return RedirectToAction("Perfil");
        }

    }
}

using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using MvcCubosGACH.Filters;
using MvcCubosGACH.Models;
using MvcCubosGACH.Services;

namespace MvcCubosGACH.Controllers {
    public class CubosController : Controller {

        private ServiceCubos service;
        private ServiceStorageBlobs serviceBlob;
        private string containerName;

        public CubosController(ServiceCubos service, ServiceStorageBlobs serviceBlob) {
            this.service = service;
            this.serviceBlob = serviceBlob;
            this.containerName = "cubopublic";
        }

        public async Task<IActionResult> GetCubos() {
            List<Cubo> cubos = await this.service.GetCubosAsync();
            foreach (Cubo c in cubos) {
                string blobName = c.Imagen;
                if (blobName != null) {
                    BlobContainerClient blobContainerClient = await this.serviceBlob.GetContainerAsync(containerName);
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                    BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                        BlobContainerName = containerName,
                        BlobName = blobName,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTime.UtcNow.AddHours(1),
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    var uri = blobClient.GenerateSasUri(sasBuilder);
                    c.Imagen = uri.ToString();
                }
            }
            return View(cubos);
        }

        [HttpPost]
        public async Task<IActionResult> GetCubos(string marca) {
            List<Cubo> cubos = await this.service.GetCubosByMarcaAsync(marca);
            foreach (Cubo c in cubos) {
                string blobName = c.Imagen;
                if (blobName != null) {
                    BlobContainerClient blobContainerClient = await this.serviceBlob.GetContainerAsync(containerName);
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                    BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                        BlobContainerName = containerName,
                        BlobName = blobName,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTime.UtcNow.AddHours(1),
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    var uri = blobClient.GenerateSasUri(sasBuilder);
                    c.Imagen = uri.ToString();
                }
            }
            return View(cubos);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cubo cubo, IFormFile file) {
            string extension = System.IO.Path.GetExtension(file.FileName);

            int newId = await this.service.InsertCuboAsync(cubo.Nombre, cubo.Marca, extension, cubo.Precio);
            string blobName = "cubo_pic_" + newId + extension;

            using (Stream stream = file.OpenReadStream()) {
                await this.serviceBlob.UploadBlobAsync(this.containerName, blobName, stream);
            }
            return RedirectToAction("GetCubos");
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Net.Mail;
using System.Net;
namespace FirmaContrato.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContratoController : ControllerBase
    {
        [HttpPost("firmar")]
        [HttpPost]

        public IActionResult FirmarContrato([FromBody] FirmaRequest request)
        {
            try
            {
                string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs/ContratoBase.pdf");
                if (!System.IO.File.Exists(pdfPath))
                    return NotFound("Plantilla PDF no encontrada.");
                // Extraer la imagen base64 de la firma
                var base64Data = Regex.Match(request.Firma, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                byte[] firmaBytes = Convert.FromBase64String(base64Data);
                using var inputPdf = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);
                var page = inputPdf.Pages[inputPdf.Pages.Count - 1];
                var gfx = XGraphics.FromPdfPage(page);
                // Cargar firma como imagen
                using var msFirma = new MemoryStream(firmaBytes);
                var img = XImage.FromStream(() => msFirma);
                // Posicionar firma y nombre
                int xFirma = 30, yFirma = 400, widthFirma = 150, heightFirma = 80;
                gfx.DrawImage(img, xFirma, yFirma, widthFirma, heightFirma);
                int xNombre = 40, yNombre = yFirma + heightFirma + 10;
                var font = new XFont("Arial", 12, XFontStyle.Regular);
                gfx.DrawString(request.Nombre ?? "", font, XBrushes.Black, new XPoint(xNombre, yNombre));
                // Guardar el PDF en memoria
                using var msPdf = new MemoryStream();
                inputPdf.Save(msPdf);
                msPdf.Position = 0;
                // Enviar por correo antes de devolver el archivo
                EnviarContratoPorCorreo(request.Nombre ?? "Usuario", msPdf.ToArray());
                // Descargar contrato en el navegador
                return File(msPdf.ToArray(), "application/pdf", "ContratoFirmado.pdf");

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private void EnviarContratoPorCorreo(string nombre, byte[] archivoPdf)
        {
            try
            {
                var mensaje = new MailMessage();
                mensaje.From = new MailAddress("lemb9206@gmail.com"); // CAMBIA aquí
                mensaje.To.Add("alcibaradrina4@gmail.com"); // CAMBIA si quieres
                mensaje.CC.Add("luismb92@outlook.com"); // Opcional
                mensaje.Subject = $"Contrato firmado por {nombre}";
                mensaje.Body = $"Hola,\n\nSe ha recibido un contrato firmado por {nombre}.\nAdjunto encontrarás el archivo.";
                // Adjuntar el PDF
                mensaje.Attachments.Add(new Attachment(new MemoryStream(archivoPdf), "ContratoFirmado.pdf"));
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("lemb9206@gmail.com", "ikgb oheg sfyo tkik"),
                    EnableSsl = true
                };
                smtp.Send(mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo: " + ex.Message);
            }
        }

    }
    public class FirmaRequest
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
        [JsonPropertyName("firma")]
        public string Firma { get; set; }
    }
}
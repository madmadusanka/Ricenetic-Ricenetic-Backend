using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Primitives;
using MLRiceneticAPI;

namespace NPNRiceModel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Configuration
            WebHost.CreateDefaultBuilder()
          .ConfigureServices(services =>
          {
                    // Register Prediction Engine Pool
                    services.AddPredictionEnginePool<NPNRiceModel.ModelInput, NPNRiceModel.ModelOutput>().FromFile("NPNRiceModel.zip");
          })
          .Configure(options =>
          {
              options.UseRouting();
              options.UseEndpoints(routes =>
              {
                        // Define prediction endpoint
                        routes.MapPost("/predict", PredictHandler);
              });
          })
          .Build()
          .Run();
        }

        static async Task PredictHandler(HttpContext http)
        {
            try
            {
                var predictionEnginePool = http.RequestServices.GetRequiredService<PredictionEnginePool<NPNRiceModel.ModelInput, NPNRiceModel.ModelOutput>>();

                // Deserialize HTTP request JSON body
                //var bodys = http.Request.Form. as Stream;
             
                //http.Request.Form.TryGetValue("ImageSource", out imagePath);
                var file = http.Request.Form.Files.GetFile("ImageSource");
                //var input = await JsonSerializer.DeserializeAsync<NPNRiceModel.ModelInput>(body);


                //var folderName = Path.Combine("Resources", "Images");
                //var pathToSave = Directory.GetCurrentDirectory();

                //var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                //var fullPath = Path.Combine(pathToSave, fileName);
                //var dbPath = Path.Combine(folderName, fileName);
                //using (var stream = new FileStream(fullPath, FileMode.Create))
                //{
                //    file.CopyTo(stream);
                //}
                var body = http.Request.Body as Stream;
                var requestBody = await JsonSerializer.DeserializeAsync<InputModel>(body);
                byte[] bytes = Convert.FromBase64String(requestBody.base64);

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }
                string fileName = "image";
                string saveDirectory = @"C:\";
                if (!Directory.Exists(fileName))
                {
                    Directory.CreateDirectory(fileName);
                }
                string fileSavePath = Path.Combine(saveDirectory, fileName);
                Image copy = image;
                copy.Save(fileSavePath, ImageFormat.Jpeg);

                var input = new NPNRiceModel.ModelInput
                {
                    ImageSource = fileSavePath
                };
                // Predict
                NPNRiceModel.ModelOutput prediction = predictionEnginePool.Predict(input);

                // Return prediction as response
                await http.Response.WriteAsJsonAsync(prediction.Prediction);
            }
            catch (Exception ex)
            {

            }
            // Get PredictionEnginePool service
            
        }
    }
}


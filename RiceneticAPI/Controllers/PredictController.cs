using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using RiceneticAPI.DataModels;
using RiceneticAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static RiceneticAPI.NPNRiceModel;

namespace RiceneticAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<NPKInput, NPKOutput> _predictionEnginePool;
        //public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);



        public PredictController(PredictionEnginePool<NPKInput, NPKOutput> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        [HttpPost]
        public ActionResult<string> Post()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {

                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Directory.GetCurrentDirectory();

                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(folderName, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }


                var input = new NPKInput()
                {
                    ImageSource =  "SFSD"
                };
                //var predictionEnginePool = RequestServices.GetRequiredService<PredictionEnginePool<NPNRiceModel.ModelInput, NPNRiceModel.ModelOutput>>();
                var n = new ModelOutput();
                var output =  _predictionEnginePool.Predict<NPKInput, NPKOutput>(input);
                //var prediction = predictService.Predict(input);


                return Ok();
            }
            catch (Exception ex)
            {
                 return BadRequest(ex);
            }
           

         
        }
    }
}

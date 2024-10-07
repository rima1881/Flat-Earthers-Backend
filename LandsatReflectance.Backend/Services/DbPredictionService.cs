using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using PredictionResults = LandsatReflectance.Backend.Services.UsgsDateTimePredictionService.PredictionResults; 

namespace LandsatReflectance.Backend.Services;

public class DbPredictionService
{
    public class PredictionDbContext : DbContext
    {
        public DbSet<Prediction> Predictions { get; set; }  
        private JsonSerializerOptions m_jsonSerializerOptions;
        
        public PredictionDbContext(DbContextOptions<PredictionDbContext> options, IOptions<JsonOptions> jsonOptions)
            : base(options)
        {
            m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new PredictionTypeConfiguration(m_jsonSerializerOptions).Configure(modelBuilder.Entity<Prediction>());
        }
    }
    
    private readonly ILogger<DbPredictionService> m_logger;
    private readonly PredictionDbContext m_predictionDbContext;
    
    private readonly UsgsApiService m_usgsApiService;
    private JsonSerializerOptions m_jsonSerializerOptions;
    
    public DbPredictionService(ILogger<DbPredictionService> logger, PredictionDbContext predictionDbContext, 
        UsgsApiService usgsApiService, IOptions<JsonOptions> jsonOptions)
    {
        m_logger = logger;
        m_predictionDbContext = predictionDbContext;

        m_usgsApiService = usgsApiService;
        m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
    }

    public IEnumerable<TargetWithPrediction> GetTargetsWithPredictions(Guid userGuid)
    {
        string sqlCommandString =
            $"""
             SELECT DISTINCT u.UserGuid, t.TargetGuid, t.ScenePath, t.SceneRow, t.Latitude, t.Longitude, t.MinCloudCover, t.MaxCloudCover, t.NotificationOffset, p.PredictionDataJson
             FROM Users AS u
             INNER JOIN UsersTargets as ut ON u.UserGuid = ut.UserGuid
             INNER JOIN Targets as t ON t.TargetGuid = ut.TargetGuid
             LEFT JOIN Predictions as p ON p.ScenePath = t.ScenePath && p.SceneRow = t.SceneRow
             WHERE u.UserGuid = '{userGuid}'
             """;

        using var sqlConnection = new MySqlConnection(m_predictionDbContext.Database.GetConnectionString());
        sqlConnection.Open();

        using var sqlCommand = new MySqlCommand(sqlCommandString, sqlConnection);
        using var reader = sqlCommand.ExecuteReader();

        var targetsWithPredictions = new List<TargetWithPrediction>();
        while (reader.Read())
        {
            var minCloudCoverOrdinal = reader.GetOrdinal("MinCloudCover");
            var maxCloudCoverOrdinal = reader.GetOrdinal("MaxCloudCover");
            var predictionDataJsonOrdinal = reader.GetOrdinal("PredictionDataJson");

            var path = reader.GetInt32("ScenePath");
            var row = reader.GetInt32("SceneRow");
            
            PredictionResults predictionResults = PredictionResults.Default;
            if (!reader.IsDBNull(predictionDataJsonOrdinal))
            {
                predictionResults = JsonSerializer.Deserialize<PredictionResults>(reader.GetString(predictionDataJsonOrdinal), m_jsonSerializerOptions)
                    ?? PredictionResults.Default;
            }
            else
            {
                predictionResults = UsgsDateTimePredictionService.Predict(m_usgsApiService, path, row).Result;
                
                var maxNotificationOffset = TimeSpan.FromDays(1);  // TODO: Make this a global constant
                var predictedAcquisitionDateTimespan = predictionResults.PredictedAcquisitionDate - DateTime.Now;
                if (predictedAcquisitionDateTimespan < maxNotificationOffset)
                {
                    // delete from database
                    var predictionsToRemove = m_predictionDbContext.Predictions.Where(prediction =>
                        prediction.Path == path && prediction.Row == row)
                        .ToList();
                    
                    m_predictionDbContext.Predictions.RemoveRange(predictionsToRemove);
                    m_predictionDbContext.SaveChanges();
                }
                else
                {
                    // add to database
                    var predictionDto = new Prediction
                    {
                        Path = path,
                        Row = row,
                        PredictionResults = predictionResults
                    };

                    m_predictionDbContext.Add(predictionDto);
                    m_predictionDbContext.SaveChanges();
                }
            }

            var targetWithPrediction = new TargetWithPrediction 
            {
                Guid = reader.GetGuid("TargetGuid"),
                Path = path,
                Row = row,
                Latitude = reader.GetDouble("Latitude"),
                Longitude = reader.GetDouble("Longitude"),
                NotificationOffset = reader.GetDateTime("NotificationOffset") - DateTime.MinValue,
                MinCloudCover = reader.IsDBNull(minCloudCoverOrdinal) ? null : reader.GetDouble(minCloudCoverOrdinal),
                MaxCloudCover = reader.IsDBNull(maxCloudCoverOrdinal) ? null : reader.GetDouble(maxCloudCoverOrdinal),
                PredictionResults = predictionResults
            };


            targetsWithPredictions.Add(targetWithPrediction);
        }

        return targetsWithPredictions;
    }
    
    
}
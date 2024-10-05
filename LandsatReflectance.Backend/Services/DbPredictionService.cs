using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

    public DbPredictionService(ILogger<DbPredictionService> logger, PredictionDbContext predictionDbContext)
    {
        m_logger = logger;
        m_predictionDbContext = predictionDbContext;
    }
}
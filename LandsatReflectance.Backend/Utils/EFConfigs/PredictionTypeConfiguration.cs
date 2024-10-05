using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using PredictionResults = LandsatReflectance.Backend.Services.UsgsDateTimePredictionService.PredictionResults;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class PredictionTypeConfiguration : IEntityTypeConfiguration<Prediction>
{
    private JsonSerializerOptions m_jsonSerializerOptions;
    
    public PredictionTypeConfiguration(JsonSerializerOptions jsonSerializerOptions)
    {
        m_jsonSerializerOptions = jsonSerializerOptions;
    }
    
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.ToTable("Predictions");
        
        builder.HasKey(prediction => new { prediction.Path, prediction.Row});

        builder.Property(prediction => prediction.Path)
            .HasColumnName("ScenePath")
            .IsRequired();
        
        builder.Property(prediction => prediction.Row)
            .HasColumnName("SceneRow")
            .IsRequired();

        builder.Property(prediction => prediction.PredictionResults)
            .HasColumnName("PredictionDataJson")
            .IsRequired()
            .HasConversion(
                predictionResults => JsonSerializer.Serialize(predictionResults, m_jsonSerializerOptions), 
                json => JsonSerializer.Deserialize<PredictionResults>(json, m_jsonSerializerOptions) ?? PredictionResults.Default);
    }
}
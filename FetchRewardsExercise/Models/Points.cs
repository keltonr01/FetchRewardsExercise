using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FetchRewardsExercise.Models
{
    // Class for binding points to spend to in PointsController.
    public class Points
    {
        [JsonPropertyName("points")]
        public int Value { get; set; }
    }
}

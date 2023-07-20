﻿using System.ComponentModel.DataAnnotations;

namespace RZDMap.DTO;

public class MapLineDTO
{
    [Required]
    public double? LatSt1 { get; set; }
    [Required]
    public double? LonSt1 { get; set; }
    [Required]
    public double? LatSt2 { get; set; }
    [Required]
    public double? LonSt2 { get; set; }
}
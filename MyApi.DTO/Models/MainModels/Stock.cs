using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyApi.DTO.Models.BaseModels;

namespace MyApi.DTO.Models.MainModels;

[Table("Stock")]
public class Stock : FullBaseModel
{
    [Key]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    public int Balance { get; set; }

}

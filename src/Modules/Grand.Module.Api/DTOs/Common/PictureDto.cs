﻿using Grand.Module.Api.Models;
using Grand.Domain.Common;

namespace Grand.Module.Api.DTOs.Common;

public class PictureDto : BaseApiEntityModel
{
    public byte[] PictureBinary { get; set; }
    public string MimeType { get; set; }
    public string SeoFilename { get; set; }
    public string AltAttribute { get; set; }
    public string TitleAttribute { get; set; }
    public Reference Reference { get; set; }
    public string ObjectId { get; set; }
    public string Style { get; set; }
    public string ExtraField { get; set; }
    public bool IsNew { get; set; }
}
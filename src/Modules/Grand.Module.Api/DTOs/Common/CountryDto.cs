﻿using Grand.Module.Api.Models;

namespace Grand.Module.Api.DTOs.Common;

public class CountryDto : BaseApiEntityModel
{
    private ICollection<StateProvinceDto> _stateProvinces;

    public string Name { get; set; }
    public bool AllowsBilling { get; set; }
    public bool AllowsShipping { get; set; }
    public string TwoLetterIsoCode { get; set; }
    public string ThreeLetterIsoCode { get; set; }
    public int NumericIsoCode { get; set; }
    public bool SubjectToVat { get; set; }
    public bool Published { get; set; }
    public int DisplayOrder { get; set; }

    public virtual ICollection<StateProvinceDto> StateProvinces {
        get => _stateProvinces ??= new List<StateProvinceDto>();
        protected set => _stateProvinces = value;
    }

    public class StateProvinceDto
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
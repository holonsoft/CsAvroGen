﻿using System;

namespace CsAvroGen.DomainModel.AvroAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AvroDefaultValueAttribute: Attribute
    {
        public object DefaultValue { get; }

        public AvroDefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}

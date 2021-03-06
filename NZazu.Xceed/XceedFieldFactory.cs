﻿using NZazu.Contracts;
using NZazu.Fields;

namespace NZazu.Xceed
{
    public class XceedFieldFactory : NZazuFieldFactory
    {
        public XceedFieldFactory(ICheckFactory checkFactory = null) 
            : base(checkFactory)
        {
            FieldTypes["string"] = typeof(XceedTextBoxField);
            FieldTypes["date"] = typeof(XceedDateTimeField);
            FieldTypes["double"] = typeof(XceedDoubleField);
            FieldTypes["int"] = typeof(XceedIntegerField);
            FieldTypes["richtext"] = typeof(XceedRichTextField);
        }
    }
}

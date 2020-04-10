﻿using System;
using System.Reflection;
using CsAvroGen.DomainModel;
using CsAvroGen.DomainModel.AvroAttributes;

namespace holonsoft.CsAvroGen.Executer
{
    internal class FieldInspector
    {
        public void Inspect(ExtendedFieldInfo efi)
        {
            var fieldType = efi.FieldInfo.FieldType;
            efi.TypeCode = Type.GetTypeCode(fieldType);


            SetTypeCodeForPrimitives(efi);

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var t = Nullable.GetUnderlyingType(fieldType);

                if (t != null)
                {
                    efi.TypeCode = Type.GetTypeCode(t);
                    efi.IsNullable = true;

                    SetTypeCodeForPrimitives(efi);
                }
            }


            if (fieldType.IsArray)
            {
                efi.TypeCode = Type.GetTypeCode(fieldType.GetElementType());
                efi.AvroType = AvroFieldType.Array;
            }


            if (fieldType.IsClass)
            {
                efi.ImplementingClassName = fieldType.Name;
            }

            if (efi.IsClass && efi.TypeCode != TypeCode.String && !fieldType.IsArray)
            {
                efi.AvroType = AvroFieldType.Record;
            }



            if (fieldType.IsGenericType)
            {
                var underlyingName = fieldType.UnderlyingSystemType.Name;

                efi.IsMap = underlyingName.Contains("Dictionary", StringComparison.InvariantCultureIgnoreCase)
                            || underlyingName.Contains("SortedList", StringComparison.InvariantCultureIgnoreCase)
                            || underlyingName.Contains("SortedDictionary", StringComparison.InvariantCultureIgnoreCase);

                if (efi.IsMap)
                {
                    if (fieldType.GenericTypeArguments[0] != typeof(string))
                    {
                        throw new NotSupportedException("key of AVRO::MAP must be a string, error in field " + efi.FieldName);
                    }

                    efi.TypeCode = Type.GetTypeCode(fieldType.GenericTypeArguments[1]);

                    efi.AvroType = AvroFieldType.Map;


                    if (efi.TypeCode == TypeCode.Object)
                    {
                        efi.AvroType = AvroFieldType.MapWithRecordType;
                    }

                }
            }


            if (fieldType.IsClass && fieldType.IsArray)
            {
                efi.ImplementingClassName = fieldType.Name.Replace("[]", string.Empty, StringComparison.InvariantCultureIgnoreCase);

                if (efi.TypeCode == TypeCode.Object)
                {
                    efi.AvroType = AvroFieldType.ArrayWithRecordType;
                }
            }


            efi.AvroDefaultValue = efi.FieldInfo.GetCustomAttribute<AvroDefaultValueAttribute>()?.DefaultValue;
            efi.HasDefaultValue = efi.AvroDefaultValue != null;


            var aliasAttr = efi.FieldInfo.GetCustomAttribute<AvroAliasAttribute>()?.AliasList;

            if (aliasAttr != null)
            {
                efi.AliasList.AddRange(aliasAttr);
            }


            var docValueAttr = efi.FieldInfo.GetCustomAttribute<AvroDocAttribute>()?.DocValue;

            if (!string.IsNullOrWhiteSpace(docValueAttr))
            {
                efi.AvroDocValue = docValueAttr;
                efi.HasDocValue = true;
            }


            var nsValueAttr = efi.FieldInfo.GetCustomAttribute<AvroNamespaceAttribute>()?.NamespaceValue;

            if (!string.IsNullOrWhiteSpace(nsValueAttr))
            {
                efi.AvroNameSpace = nsValueAttr;
                efi.HasNamespace = true;
            }


            Console.WriteLine("field " + efi.FieldName + " determined as " + efi.AvroType);

        }

        private static void SetTypeCodeForPrimitives(ExtendedFieldInfo efi)
        {
            switch (efi.TypeCode)
            {
                case TypeCode.Int32:
                    if (efi.FieldInfo.FieldType.BaseType == typeof(Enum))
                    {
                        efi.AvroType = AvroFieldType.Enum;
                    }
                    else
                    {
                        efi.AvroType = AvroFieldType.Int;
                    }
                    break;
                case TypeCode.Int64:
                    efi.AvroType = AvroFieldType.Long;
                    break;
                case TypeCode.Single:
                    efi.AvroType = AvroFieldType.Float;
                    break;
                case TypeCode.Double:
                    efi.AvroType = AvroFieldType.Double;
                    break;
                case TypeCode.String:
                    efi.AvroType = AvroFieldType.String;
                    break;
                case TypeCode.Decimal:
                    efi.AvroType = AvroFieldType.Logical;
                    break;
                case TypeCode.Byte:
                    efi.AvroType = AvroFieldType.Fixed;
                    break;
            }
        }
    }
}
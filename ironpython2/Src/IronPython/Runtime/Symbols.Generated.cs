// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.


using System;
using IronPython.Runtime.Binding;
using AnyPrefix.Microsoft.Scripting;
using AnyPrefix.Microsoft.Scripting.Generation;
using AnyPrefix.Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    public static partial class Symbols {
        internal static string OperatorToSymbol(PythonOperationKind op) {
            switch (op) {
                #region Generated StringOperatorToSymbol

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_StringOperatorToSymbol from: generate_ops.py

                case PythonOperationKind.Add: return "__add__";
                case PythonOperationKind.ReverseAdd: return "__radd__";
                case PythonOperationKind.InPlaceAdd: return "__iadd__";
                case PythonOperationKind.Subtract: return "__sub__";
                case PythonOperationKind.ReverseSubtract: return "__rsub__";
                case PythonOperationKind.InPlaceSubtract: return "__isub__";
                case PythonOperationKind.Power: return "__pow__";
                case PythonOperationKind.ReversePower: return "__rpow__";
                case PythonOperationKind.InPlacePower: return "__ipow__";
                case PythonOperationKind.Multiply: return "__mul__";
                case PythonOperationKind.ReverseMultiply: return "__rmul__";
                case PythonOperationKind.InPlaceMultiply: return "__imul__";
                case PythonOperationKind.FloorDivide: return "__floordiv__";
                case PythonOperationKind.ReverseFloorDivide: return "__rfloordiv__";
                case PythonOperationKind.InPlaceFloorDivide: return "__ifloordiv__";
                case PythonOperationKind.Divide: return "__div__";
                case PythonOperationKind.ReverseDivide: return "__rdiv__";
                case PythonOperationKind.InPlaceDivide: return "__idiv__";
                case PythonOperationKind.TrueDivide: return "__truediv__";
                case PythonOperationKind.ReverseTrueDivide: return "__rtruediv__";
                case PythonOperationKind.InPlaceTrueDivide: return "__itruediv__";
                case PythonOperationKind.Mod: return "__mod__";
                case PythonOperationKind.ReverseMod: return "__rmod__";
                case PythonOperationKind.InPlaceMod: return "__imod__";
                case PythonOperationKind.LeftShift: return "__lshift__";
                case PythonOperationKind.ReverseLeftShift: return "__rlshift__";
                case PythonOperationKind.InPlaceLeftShift: return "__ilshift__";
                case PythonOperationKind.RightShift: return "__rshift__";
                case PythonOperationKind.ReverseRightShift: return "__rrshift__";
                case PythonOperationKind.InPlaceRightShift: return "__irshift__";
                case PythonOperationKind.BitwiseAnd: return "__and__";
                case PythonOperationKind.ReverseBitwiseAnd: return "__rand__";
                case PythonOperationKind.InPlaceBitwiseAnd: return "__iand__";
                case PythonOperationKind.BitwiseOr: return "__or__";
                case PythonOperationKind.ReverseBitwiseOr: return "__ror__";
                case PythonOperationKind.InPlaceBitwiseOr: return "__ior__";
                case PythonOperationKind.ExclusiveOr: return "__xor__";
                case PythonOperationKind.ReverseExclusiveOr: return "__rxor__";
                case PythonOperationKind.InPlaceExclusiveOr: return "__ixor__";
                case PythonOperationKind.LessThan: return "__lt__";
                case PythonOperationKind.GreaterThan: return "__gt__";
                case PythonOperationKind.LessThanOrEqual: return "__le__";
                case PythonOperationKind.GreaterThanOrEqual: return "__ge__";
                case PythonOperationKind.Equal: return "__eq__";
                case PythonOperationKind.NotEqual: return "__ne__";
                case PythonOperationKind.LessThanGreaterThan: return "__lg__";

                // *** END GENERATED CODE ***

                #endregion

                // unary operators
                case PythonOperationKind.OnesComplement: return "__invert__";
                case PythonOperationKind.Negate: return "__neg__";
                case PythonOperationKind.Positive: return "__pos__";
                case PythonOperationKind.AbsoluteValue: return "__abs__";
                case PythonOperationKind.DivMod: return "__divmod__";
                case PythonOperationKind.ReverseDivMod: return "__rdivmod__";
                case PythonOperationKind.Compare: return "__cmp__";

                default:
                    throw new InvalidOperationException(op.ToString());
            }
        }

        internal static string OperatorToReversedSymbol(PythonOperationKind op) {
            switch (op) {
                case PythonOperationKind.LessThan: return "__gt__";
                case PythonOperationKind.LessThanOrEqual: return "__ge__";
                case PythonOperationKind.GreaterThan: return "__lt__";
                case PythonOperationKind.GreaterThanOrEqual: return "__le__";
                case PythonOperationKind.Equal: return "__eq__";
                case PythonOperationKind.NotEqual: return "__ne__";
                default:
                    if ((op & PythonOperationKind.Reversed) != 0) {                        
                        return OperatorToSymbol(op & ~PythonOperationKind.Reversed);
                    }

                    return OperatorToSymbol(op | PythonOperationKind.Reversed);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static PythonOperationKind OperatorToReverseOperator(PythonOperationKind op) {
            switch (op) {
                case PythonOperationKind.LessThan: return PythonOperationKind.GreaterThan;
                case PythonOperationKind.LessThanOrEqual: return PythonOperationKind.GreaterThanOrEqual;
                case PythonOperationKind.GreaterThan: return PythonOperationKind.LessThan;
                case PythonOperationKind.GreaterThanOrEqual: return PythonOperationKind.LessThanOrEqual;
                case PythonOperationKind.Equal: return PythonOperationKind.Equal;
                case PythonOperationKind.NotEqual: return PythonOperationKind.NotEqual;
                case PythonOperationKind.DivMod: return PythonOperationKind.ReverseDivMod;
                default:
                    if ((op & PythonOperationKind.Reversed) != 0) {                        
                        return op & ~PythonOperationKind.Reversed;
                    }

                    return op | PythonOperationKind.Reversed;
            }
        }

    }
}
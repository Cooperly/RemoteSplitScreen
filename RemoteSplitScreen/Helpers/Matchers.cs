using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace RemoteSplitScreen.Helpers;

public static class Matches {
    public static CodeMatch LoadsLocal(int index, bool reference = false) {
        var isShort = index < 256;

        if (reference || index > 3) {
            return new CodeMatch(Predicate);
        }

        return index switch {
            0 => new CodeMatch(OpCodes.Ldloc_0),
            1 => new CodeMatch(OpCodes.Ldloc_1),
            2 => new CodeMatch(OpCodes.Ldloc_2),
            3 => new CodeMatch(OpCodes.Ldloc_3),
            _ => new CodeMatch(Predicate)
        };

        bool Predicate(CodeInstruction instruction) {
            return CheckOpCode(instruction) && CheckOperand(instruction);
        }

        bool CheckOpCode(CodeInstruction instruction) {
            var opcode = isShort
                ? reference
                    ? OpCodes.Ldloca_S
                    : OpCodes.Ldloc_S
                : reference
                    ? OpCodes.Ldloca
                    : OpCodes.Ldloc;

            return instruction.opcode == opcode;
        }

        bool CheckOperand(CodeInstruction instruction) {
            if (instruction.operand is LocalVariableInfo lvi) {
                return lvi.LocalIndex == index;
            }

            return instruction.operand == (object) index;
        }
    }

    public static CodeMatch LoadsArgument(int index, bool reference = false) {
        var isShort = index < 256;

        if (reference || index > 3) {
            return new CodeMatch(Predicate);
        }

        return index switch {
            0 => new CodeMatch(OpCodes.Ldarg_0),
            1 => new CodeMatch(OpCodes.Ldarg_1),
            2 => new CodeMatch(OpCodes.Ldarg_2),
            3 => new CodeMatch(OpCodes.Ldarg_3),
            _ => new CodeMatch(Predicate)
        };

        bool Predicate(CodeInstruction instruction) {
            return CheckOpCode(instruction) && CheckOperand(instruction);
        }

        bool CheckOpCode(CodeInstruction instruction) {
            var opcode = isShort
                ? reference
                    ? OpCodes.Ldarga_S
                    : OpCodes.Ldarg_S
                : reference
                    ? OpCodes.Ldarga
                    : OpCodes.Ldarg;

            return instruction.opcode == opcode;
        }

        bool CheckOperand(CodeInstruction instruction) {
            var operand = instruction.operand switch {
                LocalVariableInfo lvi => lvi.LocalIndex,
                _                     => instruction.operand
            };

            if (operand == null) {
                return false;
            }

            if (isShort) {
                return operand == (object) Convert.ToByte(index);
            }

            return operand == (object) index;
        }
    }

    public static CodeMatch Branches(int label) {
        return Branches([label]);
    }

    public static CodeMatch Branches(int[] labels) {
        return new CodeMatch(it => {
            if (!Instructions.CodesBranching.Contains(it.opcode)) {
                return false;
            }

            return it.operand is Label operandLabel && labels.Contains(operandLabel.Value);
        });
    }

    /// <summary>
    ///     Lazy alternative to <see cref="Branches(int[])" />. Matches all
    ///     branch instructions without checking the operand.
    /// </summary>
    public static CodeMatch Branches() {
        return new CodeMatch(it => Instructions.CodesBranching.Contains(it.opcode));
    }

    public static CodeMatch Constructs(Type type) {
        return new CodeMatch(
            it => {
                if (it.opcode == OpCodes.Newarr && type.IsArray) {
                    return it.operand is Type arrayType && arrayType.MakeArrayType() == type;
                }

                if (it.opcode != OpCodes.Initobj && it.opcode != OpCodes.Newobj) {
                    return false;
                }

                if (it.operand is not Type objectType) {
                    return false;
                }

                return objectType == type;
            }
        );
    }

    public static CodeMatch LoadsNull() {
        return new CodeMatch(OpCodes.Ldnull);
    }

    public static CodeMatch Returns() {
        return new CodeMatch(OpCodes.Ret);
    }

    public static CodeMatch StoresLocal(int index) {
        var isShort = index < 256;

        return index switch {
            0 => new CodeMatch(OpCodes.Stloc_0),
            1 => new CodeMatch(OpCodes.Stloc_1),
            2 => new CodeMatch(OpCodes.Stloc_2),
            3 => new CodeMatch(OpCodes.Stloc_3),
            _ => new CodeMatch(Predicate)
        };

        bool Predicate(CodeInstruction instruction) {
            return CheckOpCode(instruction) && CheckOperand(instruction);
        }

        bool CheckOpCode(CodeInstruction instruction) {
            var opcode = isShort
                ? OpCodes.Stloc_S
                : OpCodes.Stloc;

            return instruction.opcode == opcode;
        }

        bool CheckOperand(CodeInstruction instruction) {
            if (instruction.operand is LocalVariableInfo lvi) {
                return lvi.LocalIndex == index;
            }

            return instruction.operand == (object) index;
        }
    }

    public static CodeMatch StoresArgument(int index) {
        var isShort = index < 256;

        return new CodeMatch(it => CheckOpCode(it) && CheckOperand(it));

        bool CheckOpCode(CodeInstruction instruction) {
            var opcode = isShort
                ? OpCodes.Starg_S
                : OpCodes.Starg;

            return instruction.opcode == opcode;
        }

        bool CheckOperand(CodeInstruction instruction) {
            return instruction.operand switch {
                LocalVariableInfo lvi => lvi.LocalIndex == index,
                byte @byte            => @byte == Convert.ToByte(index),
                _                     => instruction.operand == (object) index
            };
        }
    }

    public static CodeMatch LoadsConstant(long value) {
        return new CodeMatch(it => it.LoadsConstant(value));
    }

    public static CodeMatch LoadsConstant(double value) {
        return new CodeMatch(it => it.LoadsConstant(value));
    }

    public static CodeMatch LoadsConstant(Enum value) {
        return new CodeMatch(it => it.LoadsConstant(value));
    }

    public static CodeMatch LoadsConstant(string value) {
        return new CodeMatch(it => it.LoadsConstant(value));
    }

    public static CodeMatch LoadsConstant(bool value) {
        return new CodeMatch(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
    }

    public static CodeMatch LoadsField(Type type, string name) {
        var field = AccessTools.Field(type, name);

        if (field == null) {
            ThrowMissingField(type, name);
        }

        return LoadsField(field);
    }

    public static CodeMatch LoadsField(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.NodeType != ExpressionType.MemberAccess) {
            throw new Exception($"Expected expression to be MemberAccess, found {expression.NodeType}.");
        }

        if (expression.Member is not FieldInfo field) {
            throw new Exception(
                $"Expected FieldInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return LoadsField(field);
    }

    public static CodeMatch LoadsField(FieldInfo field) {
        return new CodeMatch(it => {
            if (field.IsStatic && it.opcode != OpCodes.Ldsfld && it.opcode != OpCodes.Ldsflda) {
                return false;
            }

            if (!field.IsStatic && it.opcode != OpCodes.Ldfld && it.opcode != OpCodes.Ldflda) {
                return false;
            }

            return Equals(it.operand, field);
        });
    }

    public static CodeMatch StoresField(Type type, string name) {
        var field = AccessTools.Field(type, name);

        if (field == null) {
            ThrowMissingField(type, name);
        }

        return StoresField(field);
    }

    public static CodeMatch StoresField(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.NodeType != ExpressionType.MemberAccess) {
            throw new Exception($"Expected expression to be MemberAccess, found {expression.NodeType}.");
        }

        if (expression.Member is not FieldInfo field) {
            throw new Exception(
                $"Expected FieldInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return StoresField(field);
    }

    public static CodeMatch StoresField(FieldInfo field) {
        return new CodeMatch(it => it.StoresField(field));
    }

    public static CodeMatch GetsProperty(Type type, string name) {
        var property = AccessTools.Property(type, name);

        if (property == null) {
            ThrowMissingProperty(type, name);
        }

        return GetsProperty(property);
    }

    public static CodeMatch GetsProperty(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.NodeType != ExpressionType.MemberAccess) {
            throw new Exception($"Expected expression to be MemberAccess, found {expression.NodeType}.");
        }

        if (expression.Member is not PropertyInfo property) {
            throw new Exception(
                $"Expected PropertyInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return GetsProperty(property);
    }

    public static CodeMatch GetsProperty(PropertyInfo property) {
        return Calls(property.GetGetMethod());
    }

    public static CodeMatch SetsProperty(Type type, string name) {
        var property = AccessTools.Property(type, name);

        if (property == null) {
            ThrowMissingProperty(type, name);
        }

        return SetsProperty(property);
    }

    public static CodeMatch SetsProperty(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.NodeType != ExpressionType.MemberAccess) {
            throw new Exception($"Expected expression to be MemberAccess, found {expression.NodeType}.");
        }

        if (expression.Member is not PropertyInfo property) {
            throw new Exception(
                $"Expected PropertyInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return SetsProperty(property);
    }

    public static CodeMatch SetsProperty(PropertyInfo property) {
        return Calls(property.GetSetMethod());
    }

    public static CodeMatch Calls(Type type, string name, Type[]? parameters = null) {
        var method = AccessTools.Method(type, name, parameters);

        if (method == null) {
            ThrowMissingMethod(type, name);
        }

        return Calls(method);
    }

    public static CodeMatch Calls(LambdaExpression lambda) {
        if (lambda.Body is not MethodCallExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MethodCallExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.NodeType != ExpressionType.Call) {
            throw new Exception($"Expected expression to be Call, found {expression.NodeType}.");
        }

        return Calls(expression.Method);
    }

    public static CodeMatch Calls(MethodInfo method) {
        return new CodeMatch(it => it.Calls(method));
    }

    public static CodeMatch Pop() {
        return new CodeMatch(OpCodes.Pop);
    }

    public static CodeMatch Dup() {
        return new CodeMatch(OpCodes.Dup);
    }

    public static CodeMatch Box(Type type) {
        return new CodeMatch(OpCodes.Box, type);
    }

    public static CodeMatch SetArrayElement() {
        return new CodeMatch(
            it => {
                return new[] {
                    OpCodes.Stelem_I,
                    OpCodes.Stelem_I1,
                    OpCodes.Stelem_I2,
                    OpCodes.Stelem_I4,
                    OpCodes.Stelem_I8,
                    OpCodes.Stelem_R4,
                    OpCodes.Stelem_R8,
                    OpCodes.Stelem_Ref,
                    OpCodes.Stelem
                }.Contains(it.opcode);
            }
        );
    }

    public static CodeMatch Nop() {
        return new CodeMatch(OpCodes.Nop);
    }

    public static CodeMatch Subtract() {
        return new CodeMatch(OpCodes.Sub);
    }

    public static CodeMatch Add() {
        return new CodeMatch(OpCodes.Add);
    }

    public static CodeMatch Compares() {
        return new CodeMatch(
            it => {
                return new[] {
                    OpCodes.Ceq,
                    OpCodes.Cgt,
                    OpCodes.Cgt_Un,
                    OpCodes.Clt,
                    OpCodes.Clt_Un
                }.Contains(it.opcode);
            }
        );
    }

    /// <summary>
    ///     Finds a subtype within a parent by its name.
    ///     This is typically required when targeting a compiler-generator closure class.
    ///     <br /><br />
    ///     More info: https://stackoverflow.com/questions/14091474/the-significance-of-in-c-sharp/14092456#14092456
    /// </summary>
    [Obsolete("This method exists for future information. Use Type.GetNestedType(string) directly instead.", true)]
    public static Type FindType(Type parent, string name) {
        return parent.GetNestedType(name);
    }

    [DoesNotReturn]
    private static void ThrowMissingField(Type type, string name) {
        throw new MissingFieldException($"No field by name \"{name}\" in \"{type.FullName}\".");
    }

    [DoesNotReturn]
    private static void ThrowMissingMethod(Type type, string name) {
        throw new MissingMethodException($"No method by name \"{name}\" in \"{type.FullName}\".");
    }

    [DoesNotReturn]
    private static void ThrowMissingProperty(Type type, string name) {
        throw new MissingPropertyException($"No property by name \"{name}\" in \"{type.FullName}\".");
    }

    private class MissingPropertyException(string message) : MissingMemberException(message);
}
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace RemoteSplitScreen.Helpers;

public static class Instructions {
    #region OpCodes

    internal static readonly OpCode[] CodesBranching = [
        OpCodes.Br_S,
        OpCodes.Brfalse_S,
        OpCodes.Brtrue_S,
        OpCodes.Beq_S,
        OpCodes.Bge_S,
        OpCodes.Bgt_S,
        OpCodes.Ble_S,
        OpCodes.Blt_S,
        OpCodes.Bne_Un_S,
        OpCodes.Bge_Un_S,
        OpCodes.Bgt_Un_S,
        OpCodes.Ble_Un_S,
        OpCodes.Blt_Un_S,
        OpCodes.Br,
        OpCodes.Brfalse,
        OpCodes.Brtrue,
        OpCodes.Beq,
        OpCodes.Bge,
        OpCodes.Bgt,
        OpCodes.Ble,
        OpCodes.Blt,
        OpCodes.Bne_Un,
        OpCodes.Bge_Un,
        OpCodes.Bgt_Un,
        OpCodes.Ble_Un,
        OpCodes.Blt_Un
    ];

    #endregion

    public static CodeInstruction Add(bool throwOverflows = true) {
        return new CodeInstruction(throwOverflows ? OpCodes.Add_Ovf : OpCodes.Add);
    }

    public static CodeInstruction Subtract(bool throwOverflows = true) {
        return new CodeInstruction(throwOverflows ? OpCodes.Sub_Ovf : OpCodes.Sub);
    }

    public static CodeInstruction Multiply(bool throwOverflows = true) {
        return new CodeInstruction(throwOverflows ? OpCodes.Mul_Ovf : OpCodes.Mul);
    }

    public static CodeInstruction Divide() {
        return new CodeInstruction(OpCodes.Div);
    }

    public static CodeInstruction Modulo() {
        return new CodeInstruction(OpCodes.Rem);
    }

    public static CodeInstruction Negate() {
        return new CodeInstruction(OpCodes.Neg);
    }

    public static CodeInstruction Duplicate() {
        return new CodeInstruction(OpCodes.Dup);
    }

    public static CodeInstruction Pop() {
        return new CodeInstruction(OpCodes.Pop);
    }

    public static CodeInstruction LoadNull() {
        return new CodeInstruction(OpCodes.Ldnull);
    }

    public static CodeInstruction Nop() {
        return new CodeInstruction(OpCodes.Nop);
    }

    public static CodeInstruction LoadString(string value) {
        return new CodeInstruction(OpCodes.Ldstr, value);
    }

    public static CodeInstruction LoadConstant(string value) {
        return LoadString(value);
    }

    public static CodeInstruction LoadConstant(bool value) {
        return new CodeInstruction(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
    }

    public static CodeInstruction LoadConstant(int value) {
        return new CodeInstruction(OpCodes.Ldc_I4, value);
    }

    public static CodeInstruction LoadConstant(long value) {
        return new CodeInstruction(OpCodes.Ldc_I8, value);
    }

    public static CodeInstruction LoadConstant(float value) {
        return new CodeInstruction(OpCodes.Ldc_R4, value);
    }

    public static CodeInstruction LoadConstant(double value) {
        return new CodeInstruction(OpCodes.Ldc_R8, value);
    }

    public static CodeInstruction LoadArgument(int index, bool reference = false) {
        return new CodeInstruction(
            reference ? OpCodes.Ldarga : OpCodes.Ldarg,
            index
        );
    }

    public static CodeInstruction BranchFalse(long address) {
        return new CodeInstruction(OpCodes.Brfalse, address);
    }

    public static CodeInstruction BranchFalse(Label label) {
        return new CodeInstruction(OpCodes.Brfalse, label);
    }

    public static CodeInstruction BranchTrue(long address) {
        return new CodeInstruction(OpCodes.Brtrue, address);
    }

    public static CodeInstruction BranchTrue(Label label) {
        return new CodeInstruction(OpCodes.Brtrue, label);
    }

    public static CodeInstruction Leave(int label) {
        return new CodeInstruction(OpCodes.Leave, LabelEx.CreateLabel(label));
    }

    public static CodeInstruction IsInstance<T>() {
        return IsInstance(typeof(T));
    }

    public static CodeInstruction IsInstance(Type type) {
        return new CodeInstruction(OpCodes.Isinst, type);
    }

    public static CodeInstruction StoreLocal(LocalBuilder builder) {
        return StoreLocal(builder.LocalIndex);
    }

    public static CodeInstruction StoreLocal(int index) {
        return new CodeInstruction(OpCodes.Stloc, index);
    }

    public static CodeInstruction LoadLocal(LocalBuilder builder) {
        return LoadLocal(builder.LocalIndex);
    }

    public static CodeInstruction LoadLocal(int index, bool reference = false) {
        return new CodeInstruction(
            reference ? OpCodes.Ldloca : OpCodes.Ldloc,
            index
        );
    }

    public static CodeInstruction Return() {
        return new CodeInstruction(OpCodes.Ret);
    }

    public static CodeInstruction Call(LambdaExpression lambda) {
        if (lambda.Body is not MethodCallExpression expression) {
            throw new Exception("Lambda body is not MethodCallExpression.");
        }

        return Call(expression.Method);
    }

    public static CodeInstruction Call(MethodInfo method) {
        return new CodeInstruction(
            method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
            method
        );
    }

    public static CodeInstruction LoadField(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.Member is not FieldInfo field) {
            throw new Exception(
                $"Expected FieldInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return LoadField(field);
    }

    public static CodeInstruction LoadField(FieldInfo field, bool reference = false) {
        var opcode = field.IsStatic
            ? reference
                ? OpCodes.Ldsflda
                : OpCodes.Ldsfld
            : reference
                ? OpCodes.Ldflda
                : OpCodes.Ldfld;

        return new CodeInstruction(opcode, field);
    }

    public static CodeInstruction GetProperty(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.Member is not PropertyInfo property) {
            throw new Exception(
                $"Expected PropertyInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return Call(AccessTools.Method(property.DeclaringType, $"get_{property.Name}"));
    }

    public static CodeInstruction StoreField(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.Member is not FieldInfo field) {
            throw new Exception(
                $"Expected FieldInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return StoreField(field);
    }

    public static CodeInstruction StoreField(FieldInfo field) {
        var opcode = field.IsStatic
            ? OpCodes.Stsfld
            : OpCodes.Stfld;

        return new CodeInstruction(opcode, field);
    }

    public static CodeInstruction SetProperty(LambdaExpression lambda) {
        if (lambda.Body is not MemberExpression expression) {
            throw new Exception(
                $"Expected lambda body to be MemberExpression, found {lambda.Body.GetType().FullName}.");
        }

        if (expression.Member is not PropertyInfo property) {
            throw new Exception(
                $"Expected PropertyInfo from MemberExpression.Member, found {expression.Member.GetType().FullName}.");
        }

        return Call(AccessTools.Method(property.DeclaringType, $"set_{property.Name}"));
    }
}
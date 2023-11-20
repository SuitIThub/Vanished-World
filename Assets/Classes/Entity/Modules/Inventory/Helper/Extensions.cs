public static class Extensions
{
    /// <summary>
    /// checks if two <see cref="Entity.Item"/> can be merged
    /// </summary>
    /// <param name="item1">The <see cref="Entity.Item"/> where <paramref name="item2"/> will be merged into.</param>
    /// <param name="item2">The <see cref="Entity.Item"/> that will be merged into <paramref name="item1"/></param>
    /// <returns>
    ///     <list type="bullet|number|table">
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
    ///             <description>The two <see cref="Entity.Item"/> can be fully merged.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.ITEM_NO_MERGE"/> (401)</term>
    ///             <description>The <paramref name="item2"/> can not be merged into <paramref name="item1"/></description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ReturnCode.codeEnum.ITEM_PARTIAL_MERGE"/> (402)</term>
    ///             <description>The <paramref name="item2"/> can be partially merged into <paramref name="item1"/></description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static ReturnCode canBeMergedWith(this Entity.Item item1, Entity.Item item2)
    {
        int item1Amount = 1;
        int item1MaxAmount = 1;
        item1.getElement("property.amount", ref item1Amount);
        item1.getElement("property.maxAmount", ref item1MaxAmount);

        int item2Amount = 1;
        int item2MaxAmount = 1;
        item2.getElement("property.amount", ref item2Amount);
        item2.getElement("property.maxAmount", ref item2MaxAmount);

        if (item1.UniqueID != item2.UniqueID)
            return ReturnCode.Code(401);

        if (item1Amount == item1MaxAmount)
            return ReturnCode.Code(401);

        int fullAmount = item1Amount + item2Amount;
        if (fullAmount < item1MaxAmount || fullAmount < item2MaxAmount)
            return ReturnCode.SUCCESS;

        return ReturnCode.Code(402);
    }
}

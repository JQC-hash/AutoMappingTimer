namespace AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static IMappingOperationOptions ExcludeMember(this IMappingOperationOptions opts, string memberName)
        {
            string MemberExclusionKey = "Ignored_Member";
            opts.Items[MemberExclusionKey] = memberName;
            return opts;
        }
    }
}


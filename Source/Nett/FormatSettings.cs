namespace Nett
{
    public enum Alignment
    {
        Immediate,
        Block,
    }

    public sealed class FormatSettings
    {
        public Alignment RowAlignment { get; private set; } = Alignment.Block;

        //public int RowBlankLines { get; set; } = 0;

        //public int AssignmentSpaces { get; set; } = 1;

        //public int ChildIndentation { get; set; } = 0;

        //public int ChildTableIndentation { get; set; } = 0;
    }
}

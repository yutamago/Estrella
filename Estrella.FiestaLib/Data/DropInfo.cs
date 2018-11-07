namespace Estrella.FiestaLib.Data
{
    public sealed class DropInfo
    {
        public DropInfo(DropGroupInfo group, float rate)
        {
            Group = group;
            Rate = rate;
        }

        public DropGroupInfo Group { get; private set; }
        public float Rate { get; private set; }
    }
}
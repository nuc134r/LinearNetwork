using System.Windows;

namespace LinearNetwork.Util
{
    public static class Extensions
    {
        public static T ToFrozen<T>(this T freezable) where T : Freezable
        {
            freezable.Freeze();
            return freezable;
        }

    }
}

namespace NxRelay
{
    public class CompositeFilter<TMessage, TFilter>(params TFilter[] filters) : Filter<TMessage>
        where TFilter : Filter<TMessage>
    {
        public override bool Apply(TMessage message)
        {
            int length = filters.Length;

            switch (length)
            {
                case 0:
                    return true;
                case 1:
                    return filters[0].Apply(message);
            }

            for (int i = 0; i < length; i++)
            {
                if (filters[i].Apply(message) == false) return false;
            }

            return true;
        }
    }
}

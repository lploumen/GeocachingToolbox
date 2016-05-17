namespace TileFetcherTest
{
    public class UncertainProperty<T>
    {
        private readonly T value;
        private readonly int certaintyLevel;
        private const int ZOOMLEVEL_MAX = 18;

        public UncertainProperty(T value) : this(value, ZOOMLEVEL_MAX)
        {

        }

        public static implicit operator T(UncertainProperty<T> obj)
        {
            return obj.value;
            //return new RomanNumeral(value);
        }

        public UncertainProperty(T value, int certaintyLevel)
        {
            this.value = value;
            this.certaintyLevel = certaintyLevel;
        }

        public T getValue()
        {
            return value;
        }

        public int getCertaintyLevel()
        {
            return certaintyLevel;
        }


        private UncertainProperty<T> getMergedProperty(UncertainProperty<T> other)
        {
            if (other == null || other.value == null)
            {
                return this;
            }
            if (this.value == null || other.certaintyLevel > certaintyLevel)
            {
                return other;
            }

            return this;
        }


        public static UncertainProperty<T> getMergedProperty(UncertainProperty<T> property, UncertainProperty<T> otherProperty)
        {
            return property == null ? otherProperty : property.getMergedProperty(otherProperty);
        }

        public static bool equalValues(UncertainProperty<T> property, UncertainProperty<T> otherProperty)
        {
            if (property == null || otherProperty == null)
            {
                return property == null && otherProperty == null;
            }
            if (property.value == null || otherProperty.value == null)
            {
                return property.value == null && otherProperty.value == null;
            }
            return property.value.Equals(otherProperty.value);
        }


    }
}
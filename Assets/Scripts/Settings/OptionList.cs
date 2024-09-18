using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    // Normally we could use simple inheritance between single parameter option list and double
    // parameter option list, but this would require to recreate inspector as serialized properties
    // are inherited even when private, that's why we are using an abstract class to wrap repetitive
    // implementation.
    public abstract class OptionList
    {
        public int Count => options.Count();

        protected abstract int defaultOptionIndex { get; }
        protected abstract IEnumerable<Option> options { get; }

        public Option defaultOption
        {
            get => options.ElementAt(defaultOptionIndex);
        }

        public List<string> choices
        {
            get
            {
                var choices = new List<string>();
                foreach (var opt in options)
                {
                    choices.Add(opt.name);
                }

                return choices;
            }
        }

        public Option this[int index]
        {
            get
            {
                if (0 <= index && index < Count)
                {
                    return options.ElementAt(index);
                }

                throw new IndexOutOfRangeException();
            }
        }

        public Option this[string name]
        {
            get
            {
                foreach (var opt in options)
                {
                    if (opt.name == name)
                    {
                        return opt;
                    }
                }

                return null;
            }
        }

        public static implicit operator List<string>(OptionList optionList) => optionList.choices;
    }

    [Serializable]
    public class OptionList<T1> : OptionList
    {
        [SerializeField] int m_DefaultOptionIndex;
        [SerializeField] List<Option<T1>> m_Options;

        protected override int defaultOptionIndex => Mathf.Clamp(m_DefaultOptionIndex, 0, m_Options.Count);
        protected override IEnumerable<Option> options => m_Options.Cast<Option>();

        public new Option<T1> defaultOption => (Option<T1>)base.defaultOption;
        public new Option<T1> this[string name] => (Option<T1>)base[name];
        public new Option<T1> this[int index] => (Option<T1>)base[index];

        public Option<T1> this[T1 value]
        {
            get
            {
                foreach (var opt in m_Options)
                {
                    if (opt.value.Equals(value))
                    {
                        return opt;
                    }
                }

                return null;
            }
        }

        public OptionList(List<Option<T1>> options, int defaultOptionIndex)
        {
            m_DefaultOptionIndex = defaultOptionIndex;
            m_Options = options;
        }
    }

    [Serializable]
    public class OptionList<T1, T2> : OptionList
    {
        [SerializeField] int m_DefaultOptionIndex;
        [SerializeField] List<Option<T1, T2>> m_Options;

        protected override int defaultOptionIndex => Mathf.Clamp(m_DefaultOptionIndex, 0, m_Options.Count);
        protected override IEnumerable<Option> options => m_Options.Cast<Option>();

        public new Option<T1, T2> defaultOption => (Option<T1, T2>)base.defaultOption;
        public new Option<T1, T2> this[string name] => (Option<T1, T2>)base[name];
        public new Option<T1, T2> this[int index] => (Option<T1, T2>)base[index];

        public Option<T1, T2> this[T1 primaryValue]
        {
            get
            {
                foreach (var opt in m_Options)
                {
                    if (opt.primaryValue.Equals(primaryValue))
                    {
                        return opt;
                    }
                }

                return null;
            }
        }

        public OptionList(List<Option<T1, T2>> options, int defaultOptionIndex)
        {
            m_DefaultOptionIndex = defaultOptionIndex;
            m_Options = options;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Utility
{
    public abstract class Setting
    {
        public event Action onChanged;

        Option m_Option;
        Func<OptionList> m_OptionListGetter;
        OptionList m_OptionList;

        public Option option
        {
            get => m_Option;
            protected set
            {
                var previousOption = m_Option;
                if (value != previousOption)
                {
                    m_Option = value;
                    onChanged?.Invoke();
                }
            }
        }

        public OptionList options => m_OptionListGetter?.Invoke() ?? m_OptionList;

        public Setting(Func<OptionList> optionListGetter)
        {
            m_OptionListGetter = optionListGetter;
            m_Option = options.defaultOption;
        }

        public Setting(OptionList optionList)
        {
            m_OptionList = optionList;
            m_Option = options.defaultOption;
        }

        public void SetOption(string name)
        {
            option = options[name] ?? options.defaultOption;
        }

        public void Reset()
        {
            option = options.defaultOption;
        }

        protected void PerformOnChanged()
        {
            onChanged?.Invoke();
        }
    }

    public class Setting<T> : Setting
    {
        public new Option<T> option => (Option<T>)base.option;
        public new OptionList<T> options => (OptionList<T>)base.options;

        public Setting(Func<OptionList<T>> optionListGetter) : base(optionListGetter) { }
        public Setting(OptionList<T> optionList) : base(optionList) { }

        public void SetOption(T value)
        {
            var option = ((OptionList<T>)options)[value];
            base.option = option ?? options.defaultOption;
        }
    }

    public class Setting<T1, T2> : Setting
    {
        public new Option<T1, T2> option => (Option<T1, T2>)base.option;
        public new OptionList<T1, T2> options => (OptionList<T1, T2>)base.options;

        public Setting(Func<OptionList<T1, T2>> optionListGetter) : base(optionListGetter) { }
        public Setting(OptionList<T1, T2> optionList) : base(optionList) { }

        public void SetOption(T1 value)
        {
            var option = ((OptionList<T1, T2>)options)[value];
            base.option = option ?? options.defaultOption;
        }
    }
}

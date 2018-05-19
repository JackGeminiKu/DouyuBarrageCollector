using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Dog
{
    // 声明一个delegate
    private EventHandler _barked = null;

    //激发事件
    public void Bark()
    {
        if (_barked != null) {
            _barked(null, null);
        }
    }

    //声明事件: 使用访问器
    public event EventHandler Barked
    {
        add
        {
            if (value != null) { _barked += value; };
        }
        remove
        {
            if (value == _barked) { _barked -= null; }
        }
    }
}
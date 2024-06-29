using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StylePosition
{
    public Position position;
    public StyleLength top;
    public StyleLength left;
    public StyleLength right;
    public StyleLength bottom;

    public StylePosition()
    {
        position = Position.Relative;
        top = StyleKeyword.Initial;
        left = StyleKeyword.Initial;
        right = StyleKeyword.Initial;
        bottom = StyleKeyword.Initial;
    }
}


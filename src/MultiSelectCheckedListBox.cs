using System.Windows.Forms.VisualStyles;

public class MultiStateCheckedListBox : ListBox
{
    private readonly Dictionary<object, CheckState> _checkStates = new();

    public MultiStateCheckedListBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        SelectionMode = SelectionMode.MultiExtended;
        DoubleBuffered = true;
    }

    public CheckState GetItemCheckState(int index)
    {
        if (index < 0 || index >= Items.Count)
            return CheckState.Unchecked;

        var item = Items[index];
        if (_checkStates.TryGetValue(item, out var state))
            return state;

        return CheckState.Unchecked;
    }

    public void SetItemCheckState(int index, CheckState state)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var item = Items[index];
        SetItemCheckState(item, state);
    }

    public void SetItemCheckState(object item, CheckState state)
    {
        if (item == null || !Items.Contains(item))
            return;

        if (state == CheckState.Unchecked)
            _checkStates.Remove(item);
        else
            _checkStates[item] = state;

        int index = Items.IndexOf(item);
        if (index >= 0)
            Invalidate(GetItemRectangle(index));
    }


    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count)
            return;

        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        CheckState checkState = GetItemCheckState(e.Index);

        Color backColor = isSelected ? SystemColors.Highlight : SystemColors.Window;
        Color textColor = isSelected ? SystemColors.HighlightText : SystemColors.ControlText;

        e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

        var boxRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 14, 14);

        if (checkState == CheckState.Checked)
        {
            CheckBoxRenderer.DrawCheckBox(e.Graphics, boxRect.Location, CheckBoxState.CheckedNormal);
        }
        else if (checkState == CheckState.Unchecked)
        {
            CheckBoxRenderer.DrawCheckBox(e.Graphics, boxRect.Location, CheckBoxState.UncheckedNormal);
        }
        else if (checkState == CheckState.Indeterminate)
        {
            CheckBoxRenderer.DrawCheckBox(e.Graphics, boxRect.Location, CheckBoxState.UncheckedDisabled);
            textColor = SystemColors.GrayText;
        }

        string itemText = Items[e.Index].ToString();
        using var textBrush = new SolidBrush(textColor);
        e.Graphics.DrawString(itemText, e.Font, textBrush, e.Bounds.X + 20, e.Bounds.Y + 1);

        e.DrawFocusRectangle();
    }

}

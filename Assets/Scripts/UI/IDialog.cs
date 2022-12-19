using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDialog
{
    void CloseDialog();
    void CloseAllOpenDialogs();
    void OpenDialog(bool closeOtherOpenDialogs);

    bool IsOpen();
    
}

﻿' Instat-R
' Copyright (C) 2015
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License k
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports instat.Translations

Public Class dlgRecodeFactor
    Private bFirstLoad As Boolean = True
    Private clsReplaceFunction, clsRevalue As RFunction
    Private bReset As Boolean = True
    Private Sub dlgRecodeFactor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If bFirstLoad Then
            InitialiseDialog()
            bFirstLoad = False
        End If
        If bReset Then
            SetDefaults()
        End If
        SetRCodeforControls(bReset)
        bReset = False
        autoTranslate(Me)
        TestOKEnabled()
    End Sub

    Private Sub InitialiseDialog()

        ucrReceiverFactor.Selector = ucrSelectorForRecode
        ucrReceiverFactor.SetIncludedDataTypes({"factor"})
        ucrReceiverFactor.SetMeAsReceiver()
        ucrBase.iHelpTopicID = 37

        ucrFactorGrid.SetReceiver(ucrReceiverFactor)
        ucrFactorGrid.bIncludeCopyOfLevels = True
        ucrFactorGrid.AddEditableColumns({ucrFactorGrid.strLabelsName})
        ucrFactorGrid.SetIsGridColumn(ucrFactorGrid.strLabelsName)

        ucrReceiverFactor.SetParameter(New RParameter("x", 0))
        ucrReceiverFactor.SetParameterIsRFunction()

        ucrFactorGrid.SetParameter(New RParameter("replace", 1))

        ucrSaveNewCol.SetSaveTypeAsColumn()
        ucrSaveNewCol.SetDataFrameSelector(ucrSelectorForRecode.ucrAvailableDataFrames)
        ucrSaveNewCol.SetIsComboBox()
        ucrSaveNewCol.SetLabelText("New Factor Column")

    End Sub

    Private Sub SetDefaults()
        clsReplaceFunction = New RFunction
        clsRevalue = New RFunction

        ucrSelectorForRecode.Reset()
        ucrSelectorForRecode.Focus()
        ucrFactorGrid.ResetText()
        ucrSaveNewCol.Reset()
        clsReplaceFunction.SetRCommand("c")
        clsReplaceFunction.AddParameter("replace", clsRFunctionParameter:=clsReplaceFunction)
        clsRevalue.SetRCommand("revalue")

        clsRevalue.SetAssignTo("recoded_column", strTempDataframe:=ucrSelectorForRecode.ucrAvailableDataFrames.cboAvailableDataFrames.Text, strTempColumn:=ucrSaveNewCol.GetText, bAssignToIsPrefix:=True)

        ucrBase.clsRsyntax.SetBaseRFunction(clsRevalue)
    End Sub

    Private Sub TestOKEnabled()
        If Not ucrReceiverFactor.IsEmpty AndAlso ucrSaveNewCol.IsComplete Then
            ucrBase.OKEnabled(True)
        Else
            ucrBase.OKEnabled(False)
        End If
    End Sub

    Private Sub SetRCodeforControls(bReset As Boolean)
        ucrReceiverFactor.SetRCode(clsRevalue, bReset)
        ucrFactorGrid.SetRCode(clsRevalue, bReset)
        ucrSaveNewCol.SetRCode(clsRevalue, bReset)
    End Sub

    Private Sub ucrBase_ClickReset(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeforControls(True)
        TestOKEnabled()
    End Sub

    Private Sub ucrReceiverFactor_ControlContentsChanged(ucrChangedControl As ucrCore) Handles ucrReceiverFactor.ControlContentsChanged, ucrSaveNewCol.ControlContentsChanged
        TestOKEnabled()
    End Sub
End Class
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

Public Class sdgOneVarUseModFit
    Private bControlsInitialised As Boolean = False
    Private clsRSeqFunction, clsOneVarRBootFunction, clsOneVarQuantileFunction, clsRPlotFunction, clsRBootFunction As New RFunction
    Public bfirstload As Boolean = True
    Private Sub sdgOneVarFitModDisplay(sender As Object, e As EventArgs) Handles MyBase.Load
        autoTranslate(Me)
    End Sub

    Public Sub InitialiseControls()
        Dim dctQuantileValues As New Dictionary(Of String, String)

        ucrNudIterations.SetParameter(New RParameter("niter", 1))
        ucrNudIterations.SetMinMax(1, 10001)
        ucrNudIterations.SetRDefault(1001)
        ucrNudIterations.Increment = 100

        ucrNudCI.SetParameter(New RParameter("CI.level", 1))
        ucrNudCI.SetMinMax(0, 1)
        ucrNudCI.SetRDefault(0.95)
        ucrNudCI.Increment = 0.05

        ucrChkParametric.SetParameter(New RParameter("bootmethod", 2), bNewChangeParameterValue:=True, bNewAddRemoveParameter:=True, strNewValueIfChecked:=Chr(34) & "param" & Chr(34), strNewValueIfUnchecked:=Chr(34) & "nonparam" & Chr(34))
        ucrChkParametric.SetText("Parametric")
        ucrChkParametric.SetRDefault(Chr(34) & "param" & Chr(34))

        ' "probs" parameter:
        ucrPnlQuantiles.AddRadioButton(rdoSequence)
        ucrPnlQuantiles.AddRadioButton(rdoInsertValues)

        '1. Function ran here is probs = seq(from = , to = , by =)
        ucrNudFrom.SetParameter(New RParameter("from", 1))
        ucrNudFrom.SetMinMax(0, 1)
        ucrNudFrom.Increment = 0.05
        ucrNudFrom.SetRDefault(0)

        ucrNudTo.SetParameter(New RParameter("to", 2))
        ucrNudTo.SetMinMax(0, 1)
        ucrNudTo.Increment = 0.05
        ucrNudFrom.SetRDefault(1)

        ucrNudBy.SetParameter(New RParameter("by", 3))
        ucrNudBy.SetMinMax(0.01, 1)
        ucrNudBy.Increment = 0.05
        ucrNudFrom.SetRDefault(0.25)

        'function ran here is probs = c(VALUES)
        ucrInputQuantiles.SetParameter(New RParameter("x"))
        dctQuantileValues.Add("0.25, 0.5, 0.75", "0.25, 0.5, 0.75")
        dctQuantileValues.Add("0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9", "0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9")
        ucrInputQuantiles.SetItems(dctQuantileValues)
        ucrInputQuantiles.SetRDefault("0.25, 0.5, 0.75")
        ucrInputQuantiles.SetValidationTypeAsNumeric(dcmMin:=0, dcmMax:=1)

        ucrPnlQuantiles.AddToLinkedControls(ucrInputQuantiles, {rdoInsertValues}, bNewLinkedAddRemoveParameter:=True, bNewLinkedDisabledIfParameterMissing:=True)
        ucrPnlQuantiles.AddToLinkedControls(ucrNudTo, {rdoSequence}, bNewLinkedAddRemoveParameter:=True, bNewLinkedDisabledIfParameterMissing:=True)
        ucrPnlQuantiles.AddToLinkedControls(ucrNudFrom, {rdoSequence}, bNewLinkedAddRemoveParameter:=True, bNewLinkedDisabledIfParameterMissing:=True)
        ucrPnlQuantiles.AddToLinkedControls(ucrNudBy, {rdoSequence}, bNewLinkedAddRemoveParameter:=True, bNewLinkedDisabledIfParameterMissing:=True)
        ucrNudTo.SetLinkedDisplayControl(lblTo)
        ucrNudFrom.SetLinkedDisplayControl(lblFrom)
        ucrNudBy.SetLinkedDisplayControl(lblBy)

        bControlsInitialised = True
    End Sub

    Public Sub SetRFunction(clsNewRSeqFunction As RFunction, clsNewRbootFunction As RFunction, clsNewQuantileFunction As RFunction, Optional bReset As Boolean = False)
        If Not bControlsInitialised Then
            InitialiseControls()
        End If
        clsRSeqFunction = clsNewRSeqFunction
        clsOneVarRBootFunction = clsNewRbootFunction
        clsOneVarQuantileFunction = clsNewQuantileFunction

        'Setting Rcode for the sub dialog
        ucrNudFrom.SetRCode(clsRSeqFunction, bReset)
        ucrNudTo.SetRCode(clsRSeqFunction, bReset)
        ucrNudBy.SetRCode(clsRSeqFunction, bReset)
        ucrChkParametric.SetRCode(clsOneVarRBootFunction, bReset)
        ucrNudIterations.SetRCode(clsOneVarRBootFunction, bReset)
        ucrNudCI.SetRCode(clsOneVarQuantileFunction, bReset)
    End Sub

    Public Sub SetDefaults()
        rdoPlotAll.Checked = True
        rdoSequence.Checked = True
        ucrInputQuantiles.Enabled = False
        SetPlotOptions()
    End Sub

    Public Sub CreateGraphs()
        If rdoPlotAll.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("plot")
            clsRPlotFunction.AddParameter("x", clsRFunctionParameter:=dlgOneVarUseModel.ucrReceiverObject.GetVariables())
        ElseIf rdoPPPlot.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("ppcomp")
            clsRPlotFunction.AddParameter("ft", clsRFunctionParameter:=dlgOneVarUseModel.ucrReceiverObject.GetVariables())
        ElseIf rdoCDFPlot.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("cdfcomp")
            clsRPlotFunction.AddParameter("ft", clsRFunctionParameter:=dlgOneVarUseModel.ucrReceiverObject.GetVariables())
        ElseIf rdoQQPlot.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("qqcomp")
            clsRPlotFunction.AddParameter("ft", clsRFunctionParameter:=dlgOneVarUseModel.ucrReceiverObject.GetVariables())
        ElseIf rdoDensityPlot.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("denscomp")
            clsRPlotFunction.AddParameter("ft", clsRFunctionParameter:=dlgOneVarUseModel.ucrReceiverObject.GetVariables())
        ElseIf rdoCIcdf.Checked Then
            clsRPlotFunction.ClearParameters()
            clsRPlotFunction.SetRCommand("CIcdfplot")
            dlgOneVarUseModel.clsRBootFunction.RemoveParameterByName("bootmethod")
            dlgOneVarUseModel.clsRBootFunction.RemoveParameterByName("niter")
            clsRPlotFunction.AddParameter("b", clsRFunctionParameter:=dlgOneVarUseModel.clsRBootFunction)
            clsRPlotFunction.AddParameter("CI.output", Chr(34) & "quantile" & Chr(34))
        End If
        frmMain.clsRLink.RunScript(clsRPlotFunction.ToScript(), iCallType:=2)
    End Sub

    Public Sub SetPlotOptions()
        If Not dlgOneVarUseModel.ucrChkProduceBootstrap.Checked Then
            rdoCIcdf.Enabled = False
            tbBootstrapOptions.Enabled = False
            If rdoCIcdf.Checked Then
                rdoPlotAll.Checked = True
            End If
        Else
            rdoCIcdf.Enabled = True
            tbBootstrapOptions.Enabled = True
        End If
    End Sub

    Private Sub ucrBase_ClickReturn(sender As Object, e As EventArgs) Handles ucrBase.ClickReturn
        dlgOneVarUseModel.QuantileCommand()
    End Sub
End Class
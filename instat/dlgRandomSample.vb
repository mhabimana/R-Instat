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
Imports instat
Imports instat.Translations

Public Class dlgRandomSample
    Private bFirstLoad As Boolean = True
    Private clsMultipleSamplesFunction As New RFunction
    Private clsDistributionFunction As New RFunction
    Private clsSetSeed As New RFunction
    Private bReset As Boolean = True

    Private Sub dlgRandomSample_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        autoTranslate(Me)
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
        ucrBase.iHelpTopicID = 376

        ucrNudNumberOfSamples.SetParameter(New RParameter("n", 0))
        ucrSampleSize.SetDataFrameSelector(ucrSelectorRandomSamples)
        ucrNudNumberOfSamples.SetMinMax(1, Integer.MaxValue)
        ucrSelectorRandomSamples.bUseCurrentFilter = False

        ucrChkSetSeed.SetText("Set Seed")
        ucrChkSetSeed.AddFunctionNamesCondition(False, "set.seed")
        ucrChkSetSeed.AddToLinkedControls(ucrNudSeed, {True}, bNewLinkedHideIfParameterMissing:=True, bNewLinkedAddRemoveParameter:=True, bNewLinkedChangeToDefaultState:=True, objNewDefaultState:=5)

        ucrNudSeed.SetParameter(New RParameter("seed", 0))
        ucrNudSeed.SetMinMax(Integer.MinValue, Integer.MaxValue)

        ucrSaveRandomSamples.SetSaveTypeAsColumn()
        ucrSaveRandomSamples.SetDataFrameSelector(ucrSelectorRandomSamples)
        ucrSaveRandomSamples.SetIsComboBox()
    End Sub

    Private Sub SetDefaults()
        clsSetSeed = New RFunction
        clsMultipleSamplesFunction = New RFunction
        clsDistributionFunction = New RFunction

        ucrSelectorRandomSamples.Reset()
        SetNewColumName()

        clsSetSeed.SetRCommand("set.seed")
        clsSetSeed.AddParameter("seed", 5)

        ucrDistWithParameters.SetRDistributions()
        ucrDistWithParameters.SetParameters()

        clsMultipleSamplesFunction.SetRCommand("replicate")
        clsMultipleSamplesFunction.AddParameter("n", 1)

        clsDistributionFunction = ucrDistWithParameters.clsCurrRFunction

        clsMultipleSamplesFunction.AddParameter("expr", clsRFunctionParameter:=clsDistributionFunction)

        ucrBase.clsRsyntax.SetBaseRFunction(clsMultipleSamplesFunction)
        ucrBase.clsRsyntax.SetAssignTo(strAssignToName:=ucrSaveRandomSamples.GetText, strTempDataframe:=ucrSelectorRandomSamples.cboAvailableDataFrames.Text, strTempColumn:=ucrSaveRandomSamples.GetText, bAssignToIsPrefix:=True)
        SetDataFrameAndDistributionParameters()
    End Sub

    Private Sub SetRCodeforControls(bReset As Boolean)
        ucrNudSeed.SetRCode(clsSetSeed, bReset)
        ucrChkSetSeed.SetRCode(clsSetSeed, bReset)
        ucrSaveRandomSamples.SetRCode(clsMultipleSamplesFunction, bReset)
        ucrNudNumberOfSamples.SetRCode(clsMultipleSamplesFunction, bReset)
    End Sub

    Private Sub TestOKEnabled()
        If ucrDistWithParameters.bParametersFilled AndAlso ucrNudNumberOfSamples.GetText <> "" _
            AndAlso (Not ucrChkSetSeed.Checked OrElse (ucrChkSetSeed.Checked AndAlso ucrNudSeed.GetText <> "")) _
            AndAlso ucrSaveRandomSamples.IsComplete Then
            ucrBase.OKEnabled(True)
        Else
            ucrBase.OKEnabled(False)
        End If
    End Sub

    Private Sub ucrBase_ClickReset(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeforControls(True)
        TestOKEnabled()
    End Sub

    Private Sub ucrBase_BeforeClickOk(sender As Object, e As EventArgs) Handles ucrBase.BeforeClickOk
        If ucrChkSetSeed.Checked Then
            frmMain.clsRLink.RunScript(clsSetSeed.ToScript(), strComment:="dlgRandomSample: Setting the seed for random number generator")
        End If
    End Sub

    Private Sub SetNewColumName()
        If ucrNudNumberOfSamples.Value = 1 Then
            ucrSaveRandomSamples.SetAssignToBooleans(bTempAssignToIsPrefix:=False)
            ucrSaveRandomSamples.SetLabelText("New Column Name:")
            If Not ucrSaveRandomSamples.bUserTyped Then
                ucrSaveRandomSamples.SetPrefix("random_sample")
            End If
        Else
            ucrSaveRandomSamples.SetAssignToBooleans(bTempAssignToIsPrefix:=True)
            ucrSaveRandomSamples.SetLabelText("Prefix for New Columns:")
            If Not ucrSaveRandomSamples.bUserTyped Then
                ucrSaveRandomSamples.SetPrefix("")
                ucrSaveRandomSamples.SetName("random_sample")
            End If
        End If
    End Sub

    Private Sub SetDataFrameAndDistributionParameters()
        If ucrDistWithParameters.clsCurrDistribution.strRName = "hyper" Then
            clsDistributionFunction.AddParameter("nn", ucrSelectorRandomSamples.iDataFrameLength)
        Else
            clsDistributionFunction.RemoveParameterByName("nn")
            clsDistributionFunction.AddParameter("n", ucrSelectorRandomSamples.iDataFrameLength)
        End If
    End Sub

    Private Sub ucrDistWithParameters_ucrInputDistributionsIndexChanged() Handles ucrDistWithParameters.DistributionsIndexChanged
        SetDataFrameAndDistributionParameters()
    End Sub

    Private Sub ucrNudNumberOfSamples_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrNudNumberOfSamples.ControlValueChanged
        SetNewColumName()
    End Sub

    Private Sub ucrSaveRandomSamples_ControlContentsChanged(ucrChangedControl As ucrCore) Handles ucrSaveRandomSamples.ControlContentsChanged, ucrSelectorRandomSamples.ControlContentsChanged, ucrChkSetSeed.ControlContentsChanged, ucrNudSeed.ControlContentsChanged, ucrSampleSize.ControlContentsChanged, ucrDistWithParameters.ControlContentsChanged
        TestOKEnabled()
    End Sub

    Private Sub ucrSelectorRandomSamples_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrSelectorRandomSamples.ControlValueChanged
        SetDataFrameAndDistributionParameters()
    End Sub
End Class
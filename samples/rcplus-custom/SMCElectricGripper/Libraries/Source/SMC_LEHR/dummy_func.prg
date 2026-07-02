'==============================================================================================
' JA: SMC LEHRシリーズ　Extensionから制御ライブラリ公開関数を呼び出すラッパー関数
' EN: SMC LEHR series  Wrapper functions for calling library functions from the Extension
'=============================================================================================

#include "defines.inc"
#include "min_max.inc"
#include "errcode.inc"


Function SMC_LEHR_Connect_(comNo As Integer) As Integer

	SMC_LEHR_Return_ = -1
    SMC_LEHR_Connect_ = SMC_LEHR_Connect(comNo)
	
	If SMC_LEHR_Connect_ <> NOERR Then
		SMC_LEHR_Return_ = SMC_LEHR_Connect_
		Exit Function
	EndIf

	' JA: パラメータセーブ無効
	' EN: Ignore parameter saving
	SMC_LEHR_disable_param_save_ = 1
	
	' JA: Jog用パラメーター書き込み
	' EN: Write Jog parameters
	' JA: レシピ11 Jog閉
	' EN: Recipe 11 Jog closing
	SMC_LEHR_SetMode(11, MODE_POS)
	SMC_LEHR_SetSpeed(11, SPD_POS_MIN)
	SMC_LEHR_SetForce(11, FORCE_MAX)

	' JA: レシピ12 Jog開
	' JA: Recipe 12 Jog opening
	SMC_LEHR_SetMode(12, MODE_POS)
	SMC_LEHR_SetSpeed(12, SPD_POS_MIN)
	SMC_LEHR_SetForce(12, FORCE_MAX)

	' JA: Jogタスク起動
	' EN: Start Jog task
	Integer taskno
	taskno = MyTask

    If 65 <= taskno And taskno <= 80 Then
    	' JA: バックグラウンドタスクとして実行されている（=Extensionから実行）とき
    	' EN: When running on background task (= Running with Extension)
		If TaskState(JogTask) = 0 Then
		    Xqt JogTask
		EndIf
	Else
		' JA: 通常タスクとして実行されている（=デバッグ用）とき
		' EN: When running on normal task (= for debug)
		If TaskState(JogTask) = 0 Then
			Xqt TaskReserve, JogTask
		EndIf
    EndIf

	SMC_LEHR_Return_ = SMC_LEHR_Connect_

Fend

Function SMC_LEHR_Disconnect_() As Integer

	SMC_LEHR_Return_ = -1
	
    ' JA: Jogタスク終了
    ' EN: Terminate Jog task
    Quit JogTask
    
    ' JA: パラメータセーブ有効
    ' EN: Enable parameter saving
	SMC_LEHR_disable_param_save_ = 0
    
    ' JA: パラメータセーブ
    ' EN: Save parameters
	MODBUS_Write(&H0589, MODBUS_ON)
	Wait SAVE_WAIT ' セーブ待ち
	MODBUS_Write(&H0589, MODBUS_OFF)
	
    SMC_LEHR_Disconnect_ = SMC_LEHR_Disconnect
    SMC_LEHR_Return_ = SMC_LEHR_Disconnect_
     
Fend

Function SMC_LEHR_Reset_() As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_Reset_ = SMC_LEHR_Reset
    SMC_LEHR_Return_ = SMC_LEHR_Reset_
Fend

Function SMC_LEHR_SetMode_(recipeNo As UByte, mode As UByte) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_SetMode_ = SMC_LEHR_SetMode(recipeNo, mode)
    SMC_LEHR_Return_ = SMC_LEHR_SetMode_
Fend

Function SMC_LEHR_GetMode_(recipeNo As UByte) As Integer
    UByte mode
	SMC_LEHR_Return_ = -1
    SMC_LEHR_GetMode_ = SMC_LEHR_GetMode(recipeNo, ByRef mode)
    
    ' JA: グリッパから読んだ値が範囲外の場合は、範囲内に書き換える
    ' EN: If the value read from the gripper is out of range, rewrite it to be within the range.
	If mode <> MODE_GRIP And mode <> MODE_POS Then
		mode = MODE_POS
	EndIf
    
    SMC_LEHR_mode_ = mode
    SMC_LEHR_Return_ = SMC_LEHR_GetMode_
Fend

Function SMC_LEHR_SetPos_(recipeNo As UByte, pos As Real) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_SetPos_ = SMC_LEHR_SetPos(recipeNo, pos)
    SMC_LEHR_Return_ = SMC_LEHR_SetPos_
Fend

Function SMC_LEHR_GetPos_(recipeNo As UByte) As Integer
    Real pos
	SMC_LEHR_Return_ = -1
    SMC_LEHR_GetPos_ = SMC_LEHR_GetPos(recipeNo, ByRef pos)
    
    ' JA: グリッパから読んだ値が範囲外の場合は、範囲内に書き換える
    ' EN: If the value read from the gripper is out of range, rewrite it to be within the range.
 	If pos < POS_MIN Then
		pos = POS_MIN
 	ElseIf POS_MAX < pos Then
	 	pos = POS_MAX
 	EndIf
    
    SMC_LEHR_pos_ = pos
    SMC_LEHR_Return_ = SMC_LEHR_GetPos_
Fend

Function SMC_LEHR_SetSpeed_(recipeNo As UByte, spd As Real) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_SetSpeed_ = SMC_LEHR_SetSpeed(recipeNo, spd)
    SMC_LEHR_Return_ = SMC_LEHR_SetSpeed_
Fend

Function SMC_LEHR_GetSpeed_(recipeNo As UByte) As Integer
    Real spd
	SMC_LEHR_Return_ = -1
    SMC_LEHR_GetSpeed_ = SMC_LEHR_GetSpeed(recipeNo, ByRef spd)
    
    ' JA: グリッパから読んだ値が範囲外の場合は、範囲内に書き換える
    ' EN: If the value read from the gripper is out of range, rewrite it to be within the range.
	If spd < SPD_POS_MIN Then
		spd = SPD_POS_MIN
	ElseIf SPD_POS_MAX < spd Then
		spd = SPD_POS_MAX
	EndIf
    
    SMC_LEHR_spd_ = spd
    SMC_LEHR_Return_ = SMC_LEHR_GetSpeed_
Fend

Function SMC_LEHR_SetForce_(recipeNo As UByte, frc As Real) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_SetForce_ = SMC_LEHR_SetForce(recipeNo, frc)
    SMC_LEHR_Return_ = SMC_LEHR_SetForce_
Fend

Function SMC_LEHR_GetForce_(recipeNo As UByte) As Integer
    Real frc
	SMC_LEHR_Return_ = -1
    SMC_LEHR_GetForce_ = SMC_LEHR_GetForce(recipeNo, ByRef frc)
    
    ' JA: グリッパから読んだ値が範囲外の場合は、範囲内に書き換える
    ' EN: If the value read from the gripper is out of range, rewrite it to be within the range.
    If frc < FORCE_MIN Then
		frc = FORCE_MIN
    ElseIf FORCE_MAX < frc Then
		frc = FORCE_MAX
    EndIf
    
    SMC_LEHR_frc_ = frc
    SMC_LEHR_Return_ = SMC_LEHR_GetForce_
Fend

Function SMC_LEHR_Servo_(OnOff As UByte) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_Servo_ = SMC_LEHR_Servo(OnOff)
    SMC_LEHR_Return_ = SMC_LEHR_Servo_
Fend

Function SMC_LEHR_Execute_(recipeNo As UByte, para As UByte) As Integer
	SMC_LEHR_Return_ = -1
    SMC_LEHR_Execute_ = SMC_LEHR_Execute(recipeNo, para)
    SMC_LEHR_Return_ = SMC_LEHR_Execute_
Fend

Function SMC_LEHR_Abort_() As Integer
	SMC_LEHR_Return_ = -1
	SMC_LEHR_Abort_ = SMC_LEHR_Abort
    SMC_LEHR_Return_ = SMC_LEHR_Abort_
Fend

Function SMC_LEHR_Status_() As Integer
    UShort sts
	SMC_LEHR_Return_ = -1
    SMC_LEHR_Status_ = SMC_LEHR_Status(ByRef sts)
    
	'JA: SMC_LEHR_sts_ はGetAllStatus内で書く
	'EN: SMC_LEHR_sts_ is changed in GetAllStatus	
    
    SMC_LEHR_Return_ = SMC_LEHR_Status_
Fend

Function SMC_LEHR_GetCurPos_() As Integer
	Real pos
	SMC_LEHR_Return_ = -1
	SMC_LEHR_GetCurPos_ = SMC_LEHR_GetCurPos(ByRef pos)
	SMC_LEHR_curpos_ = pos
    SMC_LEHR_Return_ = SMC_LEHR_GetCurPos_
Fend

Function SMC_LEHR_GetCurSpeed_() As Integer
	Real spd
	SMC_LEHR_Return_ = -1
	SMC_LEHR_GetCurSpeed_ = SMC_LEHR_GetCurSpeed(ByRef spd)
	SMC_LEHR_curspd_ = spd
    SMC_LEHR_Return_ = SMC_LEHR_GetCurSpeed_
Fend

Function SMC_LEHR_GetCurForce_() As Integer
	Real frc
	SMC_LEHR_Return_ = -1
	SMC_LEHR_GetCurForce_ = SMC_LEHR_GetCurForce(ByRef frc)
	SMC_LEHR_curfrc_ = frc
    SMC_LEHR_Return_ = SMC_LEHR_GetCurForce_
Fend



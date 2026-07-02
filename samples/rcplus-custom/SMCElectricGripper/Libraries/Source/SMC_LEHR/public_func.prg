'============================================================
' JA: SMC LEHRシリーズ　制御ライブラリ公開関数
' EN: SMC LEHR series library functions
'============================================================

#include "defines.inc"
#include "errcode.inc"
#include "min_max.inc"

'||
'|| JA: モジュール変数
'|| EN: Module variables
'||
Static Integer m_syncID		' JA: SyncLockID EN: SyncLockID
Static Integer m_abort		' JA: 中断フラグ EN: Abort flag

'
' JA: グリッパに接続
' EN: Connect to the gripper
'
Function SMC_LEHR_Connect(comNo As Integer) As Integer
	
	Integer ret

	m_abort = 0
	
	' JA: 引数チェック
	' EN: Check argument
	If Not ((COM_MIN <= comNo And comNo <= COM_MAX) Or (COM_PC_MIN <= comNo And comNo <= COM_PC_MAX)) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: COMポートオープン確認して閉じていたら開く
	' EN: Check COM port is open or not and open if closed 
	If isConnected(0) = False Then

		' JA: グローバル変数初期化
		' EN: Initalize global variable
		SMC_LEHR_mode_ = 0
		SMC_LEHR_pos_ = 0
		SMC_LEHR_spd_ = 0
		SMC_LEHR_frc_ = 0
		SMC_LEHR_sts_ = 0
		SMC_LEHR_curpos_ = 0
		SMC_LEHR_curspd_ = 0
		SMC_LEHR_curfrc_ = 0
		SMC_LEHR_commtaskno_ = 0
		SMC_LEHR_servo_onoff_ = 0
		SMC_LEHR_running_ = 0
		SMC_LEHR_jog_ = 0
		SMC_LEHR_disable_param_save_ = 0

		' JA: SyncLock ID取得 （プログラム開始時に1回のみ）
		' EN: Get SyncLick ID (Only once at program started)
		If m_syncID = 0 Then
			m_syncID = SyncLockReserve
		EndIf

		' JA: グリッパに接続
		' EN: Connect to the gripper
		SyncLock m_syncID
			ret = connect(comNo)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				disconnect()
				GoTo ErrProc
			EndIf
		SyncUnlock m_syncID
			
	Else
			
		If isConnected(comNo) = True Then
			' JA: COMポートは既にオープンしている
			' EN: COM port already opened
		Else
			ret = ERR_COM_OPEN
			GoTo ErrProc
		EndIf
		
	
	EndIf

	SMC_LEHR_Connect = NOERR
	Exit Function

ErrProc:
	
	SMC_LEHR_Connect = ret

Fend

'
' JA: グリッパと切断
' EN: Disconnect to the gripper
'
Function SMC_LEHR_Disconnect() As Integer

	disconnect()
	
	' JA: グローバル変数初期化
	' EN: Initalize global variable
	SMC_LEHR_mode_ = 0
	SMC_LEHR_pos_ = 0
	SMC_LEHR_spd_ = 0
	SMC_LEHR_frc_ = 0
	SMC_LEHR_sts_ = 0
	SMC_LEHR_curpos_ = 0
	SMC_LEHR_curspd_ = 0
	SMC_LEHR_curfrc_ = 0
	SMC_LEHR_commtaskno_ = 0
	SMC_LEHR_servo_onoff_ = 0
	SMC_LEHR_running_ = 0
	SMC_LEHR_jog_ = 0
	SMC_LEHR_disable_param_save_ = 0
	
	
	SMC_LEHR_Disconnect = NOERR

Fend

'
' JA: リセット
' EN: Reset
'
Function SMC_LEHR_Reset() As Integer

	Integer ret

	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf

	SyncLock m_syncID

		' JA: 実行フラグをすべて0にする
		' EN: Set all execution flags to 0
		UShort Addr
		ret = MODBUS_Write(&H0582, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0589, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		For Addr = &H058A To &H0599
			ret = MODBUS_Write(Addr, MODBUS_OFF)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
		Next

		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: アラームリセット
		' EN: Alarm reset
		ret = MODBUS_Write(&H0582, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0582, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: ステータス更新
		' EN: Update statuses
		UShort sts
		ret = GetAllStatus(ByRef sts)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

	SyncUnlock m_syncID
    
	SMC_LEHR_Reset = NOERR
	Exit Function
    
 ErrProc:

	SMC_LEHR_Reset = ret

Fend

'
' JA: モード設定
' EN: Set mode
'
Function SMC_LEHR_SetMode(recipeNo As UByte, mode As UByte) As Integer

	Integer ret
	UByte buf(4)

	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: モード範囲チェック
	' EN: Check the range of mode
	If Not (mode = MODE_GRIP Or mode = MODE_POS) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H0320 + 2 * (recipeNo - 1)
	
	' JA: モード
	' EN: Mode
	buf(0) = &H00
	buf(1) = &H00
	buf(2) = &H00
	Select mode
		Case MODE_GRIP
			buf(3) = MODE_GRIP	' JA: 把持 EN: Gripping
		Case MODE_POS
			buf(3) = MODE_POS	' JA: 位置決め EN: Positioning
		Default
			ret = ERR_ARGUMENT
			GoTo ErrProc
	Send
	
	SyncLock m_syncID
		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: 値設定
		' EN: Set value
		ret = MODBUS_WriteRange(addr, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: セーブ
		' EN: Save
		If SMC_LEHR_disable_param_save_ = 0 Then
			ret = MODBUS_Write(&H0589, MODBUS_ON)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
			Wait SAVE_WAIT
			ret = MODBUS_Write(&H0589, MODBUS_OFF)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
		EndIf
		
	SyncUnlock m_syncID
	
	SMC_LEHR_SetMode = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_SetMode = ret

Fend

'
' JA: モード取得
' EN: Get mode
'
Function SMC_LEHR_GetMode(recipeNo As UByte, ByRef mode As UByte) As Integer
	
	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H0320 + 2 * (recipeNo - 1)
	
	' JA: 値取得
	' EN: Get value
	SyncLock m_syncID
		ret = MODBUS_Read(addr, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc
	mode = buf(3)

	SMC_LEHR_GetMode = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_GetMode = ret
    
Fend

'
' JA: 把持/位置決め位置設定 
' EN: Set gripping/positioning position
'
Function SMC_LEHR_SetPos(recipeNo As UByte, pos As Real) As Integer

	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: 位置範囲チェック
	' EN: Check the range of position
	If Not (POS_MIN <= pos And pos <= POS_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H0280 + 2 * (recipeNo - 1)
	
	' JA: pulse → 位置[mm]
	' EN: pulse -> position[mm]
	UInt32 ps
	ps = PosToPls(pos)
	buf(0) = (ps And &HFF000000) / &H01000000
	buf(1) = (ps And &H00FF0000) / &H00010000
	buf(2) = (ps And &H0000FF00) / &H00000100
	buf(3) = (ps And &H000000FF)

	SyncLock m_syncID
		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: 値設定
		' EN: Set value
		ret = MODBUS_WriteRange(addr, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

		' JA: セーブ
		' EN: Save
		If SMC_LEHR_disable_param_save_ = 0 Then
			ret = MODBUS_Write(&H0589, MODBUS_ON)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
			Wait SAVE_WAIT
			ret = MODBUS_Write(&H0589, MODBUS_OFF)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
		EndIf

	SyncUnlock m_syncID
	
	SMC_LEHR_SetPos = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_SetPos = ret
    
Fend

'
' JA: 把持/位置決め位置取得 
' EN: Get gripping/positioning position
'
Function SMC_LEHR_GetPos(recipeNo As UByte, ByRef pos As Real) As Integer

	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H0280 + 2 * (recipeNo - 1)
	
	' JA: 値取得
	' EN: Get value
	SyncLock m_syncID
		ret = MODBUS_Read(addr, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc
	
	' JA: pulse → 位置[mm]
	' EN: pulse -> position[mm]
	UInt32 ps
	ps = buf(3)
	ps = ps + buf(2) * &H00000100
	ps = ps + buf(1) * &H00010000
	ps = ps + buf(0) * &H01000000
	pos = PlsToPos(ps)
	
	SMC_LEHR_GetPos = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_GetPos = ret
    
Fend

'
' JA: 把持/位置決め速度設定 
' EN: Set gripping/positioning speed
'
Function SMC_LEHR_SetSpeed(recipeNo As UByte, spd As Real) As Integer

	Integer ret
	UByte buf(4), bufAccel(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: 速度範囲チェック
	' EN: Check the range of speed.
	If Not (SPD_POS_MIN <= spd And spd <= SPD_POS_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス（速度、加減速度）
	' EN: Recipe No. -> Address (speed, accel/decel)
	UShort addr, addrAccel
	addr = &H02A0 + 2 * (recipeNo - 1)
	addrAccel = &H02C0 + 2 * (recipeNo - 1)
	
	' JA: 速度[mm/sec] → pulse/sec 
	' EN: Speed[mm/sec] -> pulse/sec 	
	UInt32 ps
	ps = SpdToPls(spd)
	buf(0) = (ps And &HFF000000) / &H01000000
	buf(1) = (ps And &H00FF0000) / &H00010000
	buf(2) = (ps And &H0000FF00) / &H00000100
	buf(3) = (ps And &H000000FF)
	
	' JA: 加減速 300
	' EN: Accel/decel 300
	bufAccel(0) = &H00
	bufAccel(1) = &H00
	bufAccel(2) = &H01
	bufAccel(3) = &H2C

	SyncLock m_syncID
		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
	
		' JA: 値設定（速度）
		' EN: Set value (Speed)
		ret = MODBUS_WriteRange(addr, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

		' JA: 値設定（加減速）
		' EN: Set value (Accel/decel)
		ret = MODBUS_WriteRange(addrAccel, 2, ByRef bufAccel())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: セーブ
		' EN: Save
		If SMC_LEHR_disable_param_save_ = 0 Then
			ret = MODBUS_Write(&H0589, MODBUS_ON)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
			Wait SAVE_WAIT ' セーブ待ち
			ret = MODBUS_Write(&H0589, MODBUS_OFF)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
		EndIf
		
	SyncUnlock m_syncID
	
	SMC_LEHR_SetSpeed = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_SetSpeed = ret

Fend

'
' JA: 把持/位置決め速度取得 
' EN: Get gripping/positioning speed
'
Function SMC_LEHR_GetSpeed(recipeNo As UByte, ByRef spd As Real) As Integer
	
	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス（速度）
	' EN: Recipe No. -> Address (speed)
	UShort addr
	addr = &H02A0 + 2 * (recipeNo - 1)
	
	' JA: 値取得
	' EN: Get value
	SyncLock m_syncID
		ret = MODBUS_Read(addr, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc

	' JA: pulse/sec → 速度[mm/sec] 
	' EN: pulse/sec -> Speed[mm/sec] 
	UInt32 ps
	ps = buf(3)
	ps = ps + buf(2) * &H00000100
	ps = ps + buf(1) * &H00010000
	ps = ps + buf(0) * &H01000000
	spd = PlsToSpd(ps)
	
	SMC_LEHR_GetSpeed = NOERR
	Exit Function

ErrProc:

	SMC_LEHR_GetSpeed = ret

Fend

'
' JA: 把持/位置決め力（トルク）設定
' EN: Set gripping/positioning force (torque)
'
Function SMC_LEHR_SetForce(recipeNo As UByte, frc As Real) As Integer

	UByte buf(4)
	Integer ret
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: 力範囲チェック
	' EN: Check the range of force
	If Not (FORCE_MIN <= frc And frc <= FORCE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: トルク → %
	' EN: Torque -> %
	UInt32 per
	per = TrqToPer(frc)
	buf(0) = (per And &HFF000000) / &H01000000
	buf(1) = (per And &H00FF0000) / &H00010000
	buf(2) = (per And &H0000FF00) / &H00000100
	buf(3) = (per And &H000000FF)
		
	' JA: レシピ番号→アドレス（位置決め、把持）
	' EN: Recipe No. -> Address (Positioning, Gripping)
	UShort addr_pos, addr_grip
	addr_pos = &H0300 + 2 * (recipeNo - 1)
	addr_grip = &H02E0 + 2 * (recipeNo - 1)
	
	SyncLock m_syncID
		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: 値設定（位置決め）
		' EN: Set value (Positioning)
		ret = MODBUS_WriteRange(addr_pos, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

		' JA: 値設定（把持）
		' EN: Set value (Gripping)
		ret = MODBUS_WriteRange(addr_grip, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		' JA: セーブ
		' EN: Save
		If SMC_LEHR_disable_param_save_ = 0 Then
			ret = MODBUS_Write(&H0589, MODBUS_ON)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
			Wait SAVE_WAIT ' セーブ待ち
			ret = MODBUS_Write(&H0589, MODBUS_OFF)
			If ret <> NOERR Then
				SyncUnlock m_syncID
				GoTo ErrProc
			EndIf
		EndIf
		
	SyncUnlock m_syncID
	
	SMC_LEHR_SetForce = NOERR
 	Exit Function

ErrProc:

	SMC_LEHR_SetForce = ret
    
Fend

'
' JA: 把持/位置決め力（トルク）取得
' EN: Get gripping/positioning force (torque)
'
Function SMC_LEHR_GetForce(recipeNo As UByte, ByRef frc As Real) As Integer

	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf
	
	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H0300 + 2 * (recipeNo - 1)
	
	' JA: 値取得
	' EN: Get value
	SyncLock m_syncID
		ret = MODBUS_Read(addr, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc

	' JA: % → トルク
	' EN: % -> Torque
	UInt32 per
	per = buf(3)
	per = per + buf(2) * &H00000100
	per = per + buf(1) * &H00010000
	per = per + buf(0) * &H01000000
	frc = PerToTrq(per)
	
	SMC_LEHR_GetForce = NOERR
 	Exit Function
    
ErrProc:

	SMC_LEHR_GetForce = ret
    
Fend


'
' JA: サーボOn/Off
' EN: Servo On/Off
'
Function SMC_LEHR_Servo(OnOff As UByte) As Integer
	
	Integer ret, loopcnt
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	Select OnOff
		
		Case SERVO_ON
			SyncLock m_syncID
				ret = MODBUS_Write(&H0580, MODBUS_ON)
				If ret <> NOERR Then
					SyncUnlock m_syncID
					GoTo ErrProc
				EndIf
				
				' JA: モーターステータスの「サーボオフ」(bit4)が0になるまで待つ
				' EN: Wait until the motor status "Servo Off" (bit 4) changes to 0
				loopcnt = 10
				Do
					ret = MODBUS_Read(&H0108, 2, ByRef buf())
					loopcnt = loopcnt - 1
				Loop While ((buf(3) And &B00010000) > 0) And (loopcnt > 0) And (ret = NOERR)
				If loopcnt = 0 Then ret = ERR_COMMUNICATION
				If ret <> NOERR Then
					SyncUnlock m_syncID
					GoTo ErrProc
				EndIf
				
				SMC_LEHR_servo_onoff_ = SERVO_ON
			SyncUnlock m_syncID
			
		Case SERVO_OFF
			SyncLock m_syncID
				ret = MODBUS_Write(&H0580, MODBUS_OFF)
				If ret <> NOERR Then
					SyncUnlock m_syncID
					GoTo ErrProc
				EndIf

				' JA: モーターステータスの「サーボオフ」(bit4)が1になるまで待つ
				' EN: Wait until the motor status "Servo Off" (bit 4) changes to 1
				loopcnt = 10
				Do
					ret = MODBUS_Read(&H0108, 2, ByRef buf())
					loopcnt = loopcnt - 1
				Loop While ((buf(3) And &B00010000) = 0) And (loopcnt > 0) And (ret = NOERR)
				If loopcnt = 0 Then ret = ERR_COMMUNICATION
				If ret <> NOERR Then
					SyncUnlock m_syncID
					GoTo ErrProc
				EndIf

				SMC_LEHR_servo_onoff_ = SERVO_OFF
			SyncUnlock m_syncID
			
		Default
			' JA: 範囲外エラー
			' EN: Error for argument out of range
			ret = ERR_ARGUMENT
			GoTo ErrProc
	Send
	
	SMC_LEHR_Servo = NOERR
 	Exit Function
    
ErrProc:

	SMC_LEHR_Servo = ret
    
Fend


'
' JA: レシピ実行
' EN: Execute recipe
'
Function SMC_LEHR_Execute(recipeNo As UByte, para As UByte) As Integer

	UByte buf(4)
	Integer ret
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: 中断フラグを0クリアする
	' EN: Clear abort flag to 0
	m_abort = 0

	' JA: レシピ範囲チェック
	' EN: Check the range of recipeNo.
	If Not (RECIPE_MIN <= recipeNo And recipeNo <= RECIPE_MAX) Then
		ret = ERR_ARGUMENT
		GoTo ErrProc
	EndIf

	' JA: レシピ番号→アドレス
	' EN: Recipe No. -> Address
	UShort addr
	addr = &H058A + (recipeNo - 1)

	SyncLock m_syncID
		
		' JA: レシピ実行OFF（念のため）
		' EN: Recipe Execute OFF (just in case)		
		ret = MODBUS_Write(addr, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

		' JA: レシピ実行
		' EN: Recipe Execute	
		ret = MODBUS_Write(addr, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf

		ret = MODBUS_Write(addr, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
			
		' JA: サーボONであるか確認
		' EN: Check Servo ON
		ret = MODBUS_Read(&H0108, 2, ByRef buf())
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
			
		If (buf(3) And &B00010000) = 0 Then	 ' JA: モーターステータスの「サーボオフ」(bit4)が0を確認　EN: Check the motor status "Servo Off" (bit 4) is 0
	
			' JA: インポジ待ち
			' EN: Wait for inposition
			If para = 0 Then
			
				SMC_LEHR_running_ = 1
			
				Do
					ret = MODBUS_Read(&H0108, 2, ByRef buf())
					If ret <> NOERR Then
						SyncUnlock m_syncID
						GoTo ErrProc
					EndIf

					' JA: SMC_LEHR_Abort()発行でループから抜ける
					' EN: Exit the loop when SMC_LEHR_Abort() called 
					If m_abort > 0 Then Exit Do
					
					' JA: コンテキストスイッチ
					' EN: For context switch
					Wait 0

				Loop While (buf(3) And &B00001000) = 0
				
			EndIf
			
		Else	 ' JA: サーボがOFFだった場合  EN: If Servo is Off 
			ret = ERR_SERVO_OFF
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		
		SMC_LEHR_running_ = 0
		
	SyncUnlock m_syncID

	SMC_LEHR_Execute = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_running_ = 0
	SMC_LEHR_Execute = ret

Fend


'
' JA: 中断
' EN: Abort
'
Function SMC_LEHR_Abort() As Integer
	
	Integer ret
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: 中断フラグに1をセットする
	' EN: Set abort flag to 1
	m_abort = 1
	
	SyncLock m_syncID
		' JA: 停止
		' EN: Stop
		ret = MODBUS_Write(&H0581, MODBUS_ON)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
		ret = MODBUS_Write(&H0581, MODBUS_OFF)
		If ret <> NOERR Then
			SyncUnlock m_syncID
			GoTo ErrProc
		EndIf
	SyncUnlock m_syncID
	
 	SMC_LEHR_Abort = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_Abort = ret

Fend

'
' JA: ステータス取得
' EN: Get statuses
'
Function SMC_LEHR_Status(ByRef sts As UShort) As Integer

	Integer ret
	UByte buf(8)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf

	' JA: 全ステータス取得
	' EN: Get all statuses
	SyncLock m_syncID
		ret = GetAllStatus(ByRef sts)
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc
	
	SMC_LEHR_Status = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_Status = ret

Fend

'
' JA: 現在位置取得
' EN: Get current position
'
Function SMC_LEHR_GetCurPos(ByRef pos As Real) As Integer
	
	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: 現在位置取得
	' EN: Get current position
	SyncLock m_syncID
		ret = MODBUS_Read(&H0102, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc
	
	' JA: pulse → 位置[mm]
	' EN: pulse -> position[mm]	
	UInt32 ps
	ps = buf(3)
	ps = ps + buf(2) * &H00000100
	ps = ps + buf(1) * &H00010000
	ps = ps + buf(0) * &H01000000
	pos = PlsToPos(ps)
	
	SMC_LEHR_GetCurPos = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_GetCurPos = ret

Fend

'
' JA: 現在速度取得
' EN: Get current speed
'
Function SMC_LEHR_GetCurSpeed(ByRef spd As Real) As Integer
	
	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: 現在速度取得
	' EN: Get current speed	
	SyncLock m_syncID
		ret = MODBUS_Read(&H0104, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc

	' JA: pulse/sec →　mm/sec
	' EN: pulse/sec ->　mm/sec	
	UInt32 ps
	ps = buf(3)
	ps = ps + buf(2) * &H00000100
	ps = ps + buf(1) * &H00010000
	ps = ps + buf(0) * &H01000000
	spd = PlsToSpd(ps)
	
	SMC_LEHR_GetCurSpeed = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_GetCurSpeed = ret
	
Fend

'
' JA: 現在力（トルク）取得
' EN: Get current force (torque)
'
Function SMC_LEHR_GetCurForce(ByRef frc As Real) As Integer
	
	Integer ret
	UByte buf(4)
	
	' JA: 接続確認
	' EN: Check connection
	If isConnected(0) = False Then
		ret = ERR_COM_OPEN
		GoTo ErrProc
	EndIf
	
	' JA: 現在力（トルク）取得
	' EN: Get current force (torque)	
	SyncLock m_syncID
		ret = MODBUS_Read(&H0106, 2, ByRef buf())
	SyncUnlock m_syncID
	If ret <> NOERR Then GoTo ErrProc
	
	' JA: % → トルク
	' EN: % -> Torque
	UInt32 per
	per = buf(3)
	per = per + buf(2) * &H00000100
	per = per + buf(1) * &H00010000
	per = per + buf(0) * &H01000000

	' JA: 現在トルク[%] = 取得したトルク[%] / 0.8
	' EN: Current torque[%] = Recieved Torque[%] / 0.8
	frc = PerToTrq(per) /0.8
	
	SMC_LEHR_GetCurForce = NOERR
	Exit Function
	
ErrProc:

	SMC_LEHR_GetCurForce = ret
	
Fend



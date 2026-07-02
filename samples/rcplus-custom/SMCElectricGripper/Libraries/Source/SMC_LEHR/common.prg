'===================================================================================
' JA: SMC LEHRシリーズ　制御ライブラリ　共通関数（ライブラリ内使用）
' EN: SMC LEHR series　Librarycommon functions (only refered into the library)
'===================================================================================

#include "defines.inc"
#include "errcode.inc"


'||
'|| JA: モジュール変数（フラグ）
'|| EN: Module variables (flag)
'||
Static Integer m_exec		' JA: Modbus送受信実行 EN: Modbus send/receive execution
Static Integer m_finish		' JA: Modbus送受信完了 0:未完了 >0:成功 -1:CRCエラー -2:受信タイムアウト　-9:例外 EN: Modbus send/receive completion 0:Not completed >0:Success -1:CRC error -2:Recieve timeout
Static Integer m_init		' JA: 通信初期化完了　0:未完了 >0:成功 -9:例外（Openエラー）	EN: Communication initialization complete 0:Not completed >0:Success -9:Exception(Open error)
Static Integer m_connect	' JA: 通信確立 EN: Communication established

'||
'|| JA: モジュール変数
'|| EN: Module variables
'||
Static Integer m_taskno_NormalTask		' JA: 通信タスクID（通常タスク起動時） EN: Communitacion Task ID (when running in normal task)
Static Integer m_port					' JA: オープンしたCOMポート番号  JA: Opened COM port number

'||
'|| JA: モジュール変数（送受信データバッファ）
'|| EN: Module variables (Send / Recieve data buffer)
'||
Static UByte m_mbusTxBuf(64)		' JA: Modbus送信バッファ EN: Modbus send buffer
Static UShort m_mbusTxBufLen		' JA: Modbus送信バッファByte数 EN: Bytes for Modbus send buffer
Static UByte m_mbusRxBuf(64)		' JA: Modbus受信バッファ EN: Modbus recieve buffer
Static UShort m_mbusRxBufLen		' JA: Modbus受信バッファByte数（予定値）EN: Bytes for Modbus recieve buffer (Target value)

'||
'|| JA: Waitコマンドに渡すフラグ変数の変更マクロ
'|| JA: 備考：Waitコマンドがフラグ変化を認識するための待ち時間を伴う。　Waitコマンドのヘルプを参照
'|| EN: Macro for changing flag variables passing to Wait command
'|| EN: Note: The Wait command involves a waiting period to recognize the flag change. See the Wait command help.
'||
#define FLAG_ON(flagVarName) flagVarName = 1; Wait 0.02				' JA: フラグ変数をONし、20ms待つ EN: Turn flag variable ON and wait 20 ms
#define FLAG_OFF(flagVarName) flagVarName = 0; Wait 0.02			' JA: フラグ変数をOFFし、20ms待つ EN: Turn flag variable OFF and wait 20 ms
#define FLAG_SET(flagVarName, value) flagVarName = value; Wait 0.02	' JA: フラグ変数を変更し、20ms待つ EN: Change flag variable and wait 20 ms



'
' JA: CRC16計算
' EN: CRC16 calculation
'
Function CalcCRC16(ByRef data() As UByte, count As Integer) As UShort

    UShort crc
    Integer i, j

    crc = &HFFFF

    For i = 0 To count - 1

        crc = crc Xor data(i)

        For j = 0 To 7
            If (crc And 1) > 0 Then
                crc = crc /2
                crc = crc Xor &HA001
            Else
                crc = crc /2
            EndIf
            
        Next j

		Wait 0
        

    Next i

    CalcCRC16 = crc

Fend

'
' JA: グリッパと接続（内部処理）
' EN: Connect to the gripper (internal process)
'
Function connect(comNo As Integer) As Integer

	' JA: モジュール変数初期化
	' EN: Initalize module variables	
	m_exec = 0
	m_finish = 0
	m_init = 0
	m_port = 0
	
	Integer taskno
	taskno = MyTask
	
	' JA: 通信タスク開始
	' EN: Start communication task
    If 65 <= taskno And taskno <= 80 Then

    	' JA: バックグラウンドタスクとして実行されている（=Extensionから実行）とき
    	' EN: When running on background task (= Running with Extension)
		If TaskState(CommunicationTask) = 0 Then
		    Xqt CommunicationTask(comNo)
		EndIf

	Else

		' JA: 通常タスクとして実行されている（=SPELから利用）とき
		' EN: When running on normal task (= Called by SPEL)		
		If TaskState(CommunicationTask) = 0 Then
			Xqt TaskReserve, CommunicationTask(comNo), NoEmgAbort
    	EndIf
    
    EndIf

    Wait m_init <> 0

    If m_init > 0 Then
    
		Integer ret

		' JA: 通信テスト（ステータス取得）
		' JA: Communication test (Get status)		
		UShort sts
		ret = GetAllStatus(ByRef sts)
		If ret <> NOERR Then
			disconnect()
	    	connect = ret
	    	Exit Function
		EndIf

		m_port = comNo
		connect = NOERR

    Else
    	connect = ERR_COM_OPEN
    EndIf
    
Fend

'
' JA: ポートオープンしているか否か判断
' JA: m_port = 0 のとき、SMC_LEHR_Connectを実行していない
' JA: port = 0 のとき、OpenCom(m_port) > 0ならオープンしていると判断
' JA: port <> 0 のとき、port = m_portならオープンしていると判断
' EN: Determine whether the port is open or not
' EN: m_port = 0 ->　Does not execute SMC_LEHR_Connect
' EN: port = 0  -> Judge opened when OpenCom(m_port) > 0
' EN: port <> 0  -> Judge opened when port = m_port
'
Function isConnected(port As Integer) As Boolean

	OnErr GoTo ErrProc

	If m_port = 0 Then
		isConnected = False
	ElseIf port = 0 Then
		If OpenCom(m_port) > 0 Then
			isConnected = True
		EndIf
	ElseIf port = m_port Then
		isConnected = True
	Else
		isConnected = False
	EndIf

	Exit Function

ErrProc:

	isConnected = False

Fend

'
' JA: グリッパと切断（内部処理）
' EN: Disconnect to the gripper (Internal process)
'
Function disconnect()

	OnErr GoTo ErrProc

	Quit CommunicationTask
	If OpenCom(m_port) >= 0 Then
		CloseCom #m_port
	EndIf
	
	m_port = 0
	
	Exit Function
	
ErrProc:

	m_port = 0
	
Fend

'
' JA: 通信タスク
' EN: Communication task
'
Function CommunicationTask(comNo As Integer)
	
	OnErr GoTo ErrProc
	
	Integer timeoutcnt

	' JA: COMポートオープン
	' EN: Open COM port	
    OpenCom #comNo
	If ChkCom(comNo) < 0 Then
		FLAG_SET(m_init, -1)
		Exit Function
	EndIf
	
	' JA: タスク番号取得
	' EN: Get task No.
	SMC_LEHR_commtaskno_ = MyTask
	
	' JA: 初期化完了通知
	' EN: Notification for initalize process completed
	FLAG_ON(m_init)
	
	' JA: 送受信ループ（通信終了時はタスクが強制終了される）
	' EN: Loop for sending / recieving (The task is killed when disconnecting) 	
    Do
    	
		' JA: 受信バッファクリア
		' EN: Clear recieveing buffer
		Integer cnt
		UByte dummy(16)
		
		cnt = ChkCom(comNo)
		Do While cnt > 0
			If cnt > 16 Then cnt = 16
			ReadBin #comNo, dummy(), cnt
			Wait 0.1
			cnt = ChkCom(comNo)
		Loop
		
		' JA: 送信待ち
		' EN: Wait for sending
		Wait m_exec > 0

		' JA: Modbusフレーム送信
		' EN: Send Modbus frame
		WriteBin #comNo, m_mbusTxBuf(), m_mbusTxBufLen
		
		' JA: 受信待ち
		' EN: Wait for recieving
		Wait MODBUS_RECV_WAIT
		timeoutcnt = 10
		Do While (ChkCom(comNo) < m_mbusRxBufLen) And (timeoutcnt > 0)
			Wait 0.1
			timeoutcnt = timeoutcnt - 1
		Loop
		If timeoutcnt > 0 Then
		
			' JA: 受信フレーム取得
			' EN: Get recieving frame
			ReadBin #comNo, m_mbusRxBuf(), m_mbusRxBufLen
			
			' JA: CRCチェック
			' EN: CRC check
			UShort crc, crc_upper, crc_lower
			crc = CalcCRC16(ByRef m_mbusRxBuf(), m_mbusRxBufLen - 2)
			crc_upper = (crc And &HFF00) / &H0100
			crc_lower = (crc And &H00FF)
			If m_mbusRxBuf(m_mbusRxBufLen - 1) = crc_upper And m_mbusRxBuf(m_mbusRxBufLen - 2) = crc_lower Then
				' JA: 受信成功
				' EN: Recieving success
				' JA: 完了通知（成功）
				' EN: Notification of completed (Success)
				FLAG_ON(m_finish)
			Else
				' 受信データーCRCチェックエラー
				' 完了通知（CRCチェックエラー）
				FLAG_SET(m_finish, -1)
			EndIf
		
		Else
			Integer recv
			recv = ChkCom(comNo)
			
			' JA: 受信タイムアウト
			' EN: Receiving timeout
			If recv > 0 Then
			
				' JA: 受信フレーム取得(空読み)
				' EN: Get recieving frame(Dummy read)
				ReadBin #comNo, m_mbusRxBuf(), recv
			
				' JA: 完了通知（受信タイムアウト）
				' EN: Notification of completed (Recieve timeout)
				FLAG_SET(m_finish, -2)
				
			Else

				' JA: 完了通知（無応答）
				' EN: Notification of completed (No response)				
				FLAG_SET(m_finish, -3)

			EndIf
			
				
		EndIf
		
		' JA: 処理待ち
		' EN: Wait for processing completed 
		Wait m_exec = 0
		
		' JA: 完了通知（成功）
		' EN: Notification of completed (Success)
		FLAG_OFF(m_finish)
		
		' JA: コンテキストスイッチ
		' EN: For context switch
		Wait 0

	Loop

	GoTo End

ErrProc:

	' JA: 例外終了
	' EN: Terminated by exception
	Integer errno
	errno = Err
	If errno > 0 Then
	    ' JA: 例外発生
		' EN: Exceoption occured
	EndIf
	
	' JA: 初期化完了通知（例外）
	' EN: Notification for initalize process completed (Exception occured)
	FLAG_SET(m_init, -9)
	
	' JA: 完了通知（例外）
	' EN: Notification of complete (Exception occured)
	FLAG_SET(m_finish, -9)

End:

	' JA: 通信終了
	' EN: Communication terminated
	If ChkCom(comNo) >= 0 Then
		CloseCom #comNo
	EndIf

Fend

'
' JA: 保持レジスタ読出し
' EN: Read holding register
'
Function MODBUS_Read(Addr As UShort, numReg As UShort, ByRef reg() As UByte) As Integer

	UShort crc, bytes

	' JA: 送信フレーム生成
	' EN: Create sending frame	
	m_mbusTxBuf(0) = MODBUS_ADDR			' JA: スレーブID EN: Slave ID
	m_mbusTxBuf(1) = MBUS_FUNC_READ			' JA: ファンクションNo. EN: Function No.
	m_mbusTxBuf(2) = Addr / &H0100			' JA: 開始アドレス（上位） EN: Start address (upper byte)
	m_mbusTxBuf(3) = Addr And &H00FF		' JA: 開始アドレス（下位） EN: Start address (lower byte)
	m_mbusTxBuf(4) = numReg / &H0100		' JA: レジスタ数（上位） EN: Number of register (upper byte)
	m_mbusTxBuf(5) = numReg And &H00FF		' JA: レジスタ数（下位） EN: Number of register (lower byte)
	crc = CalcCRC16(ByRef m_mbusTxBuf(), 6)	' JA: CRC計算 EN: CRC Calculation
	m_mbusTxBuf(6) = crc And &H00FF			' JA: CRC（下位） EN: CRC (lower byte)
	m_mbusTxBuf(7) = crc / &H0100			' JA: CRC（上位） EN: CRC (upper byte) 

	m_mbusTxBufLen = 8
	bytes = 2 * numReg
	m_mbusRxBufLen = 3 + bytes + 2			' JA: 受信予定バイト数 EN: Number of bytes expected to be received

	' JA: 送信
	' EN: Sending
	FLAG_ON(m_exec)
	
	' JA: 受信待ち
	' EN: Wait for recieving
	Wait m_finish <> 0
	
	If m_finish > 0 Then	 ' JA: 受信成功 EN: Recieve success

		' JA: バッファにコピー
		' EN: Copy to the buffer
		Integer i
		For i = 0 To bytes - 1
			reg(i) = m_mbusRxBuf(3 + i)
		Next
		
		MODBUS_Read = NOERR
	Else	 ' JA: 受信エラー EN: Recieve fail 
		MODBUS_Read = ERR_COMMUNICATION
	EndIf
	
	FLAG_OFF(m_exec)

Fend

'
' JA: 単一コイル書込み
' EN: Write Single coil
'
Function MODBUS_Write(Addr As UShort, Value As UShort) As Integer

	UShort crc

	' JA: 送信フレーム生成
	' EN: Create sending frame
	m_mbusTxBuf(0) = MODBUS_ADDR			' JA: スレーブID EN: Slave ID
	m_mbusTxBuf(1) = MBUS_FUNC_WRITE		' JA: ファンクションNo. EN: Function No.
	m_mbusTxBuf(2) = Addr / &H0100			' JA: 開始アドレス（上位） EN: Start address (upper byte)
	m_mbusTxBuf(3) = Addr And &H00FF		' JA: 開始アドレス（下位） EN: Start address (lower byte)
	m_mbusTxBuf(4) = Value / &H0100			' JA: 書き込み値（上位） EN: Value to write (upper byte)
	m_mbusTxBuf(5) = Value And &H00FF		' JA: 書き込み値（下位） EN: Value to write (lower byte)
	crc = CalcCRC16(ByRef m_mbusTxBuf(), 6)	' JA: CRC計算 EN: CRC Calculation
	m_mbusTxBuf(6) = crc And &H00FF			' JA: CRC（下位） EN: CRC (lower byte)
	m_mbusTxBuf(7) = crc / &H0100			' JA: CRC（上位） EN: CRC (upper byte)
	
	m_mbusTxBufLen = 8
	m_mbusRxBufLen = 8		' JA: 受信予定バイト数 EN: Number of bytes expected to be received

	' JA: 送信
	' EN: Sending
	FLAG_ON(m_exec)
	
	' JA: 受信待ち
	' EN: Wait for recieving
	Wait m_finish <> 0

	If m_finish > 0 Then	' JA: 受信成功 EN: Recieve success
		MODBUS_Write = NOERR
	Else 					' JA: 受信エラー EN: Recieve fail 
		MODBUS_Write = ERR_COMMUNICATION
	EndIf
					
	FLAG_OFF(m_exec)
		
Fend

'
' JA: 複数保持レジスタ書込み
' EN: Write multiple holding register
'
Function MODBUS_WriteRange(Addr As UShort, numReg As UShort, ByRef reg() As UByte) As Integer

	UShort crc, bytes
	Integer i
	
	' JA: 送信フレーム生成
	' JA: Create sending frame
	m_mbusTxBuf(0) = MODBUS_ADDR				' JA: スレーブID EN: Slave ID
	m_mbusTxBuf(1) = MBUS_FUNC_WRITERANGE		' JA: ファンクションNo EN: Function No.
	m_mbusTxBuf(2) = Addr / &H0100				' JA: 開始アドレス（上位） EN: Start address (upper byte)
	m_mbusTxBuf(3) = Addr And &H00FF			' JA: 開始アドレス（下位） EN: Start address (lower byte)
	m_mbusTxBuf(4) = numReg / &H0100			' JA: レジスタ数（上位） EN: Number of registers (upper byte)
	m_mbusTxBuf(5) = numReg And &H00FF			' JA: レジスタ数（下位） EN: Number of registers (lower byte)
	bytes = 2 * numReg							' JA: データByte数（= 2 * レジスタ数）EN: Bytes of registers (= 2 * Number of registers)
	m_mbusTxBuf(6) = bytes
	For i = 0 To bytes - 1
		m_mbusTxBuf(7 + i) = reg(i)				' JA: 書込み値 EN: values to write
	Next

	crc = CalcCRC16(ByRef m_mbusTxBuf(), 7 + bytes)	' JA: CRC計算 EN: CRC calculation
	m_mbusTxBuf(7 + bytes) = crc And &H00FF			' JA: CRC（下位） EN: CRC (lower byte)
	m_mbusTxBuf(7 + bytes + 1) = crc / &H0100		' JA: CRC（下位） EN: CRC (upper byte)
	
	m_mbusTxBufLen = 7 + bytes + 2		' JA: Number of bytes to send
	m_mbusRxBufLen = 8 					' JA: 受信予定バイト数 EN: Number of bytes expected to be received

	' JA: 送信
	' EN: Sending
	FLAG_ON(m_exec)
	
	' JA: 受信待ち
	' EN: Wait for recieving
	Wait m_finish <> 0
	
	If m_finish > 0 Then	' JA: 受信成功 EN: Recieve success
		MODBUS_WriteRange = NOERR
	Else 					' JA: 受信エラー EN: Recieve fail 
		MODBUS_WriteRange = ERR_COMMUNICATION
	EndIf

	FLAG_OFF(m_exec)
	

Fend

'
' JA: ステータス取得、グローバル変数セット
' EN: Get status and set global variables
'
Function GetAllStatus(ByRef sts As UShort) As Integer

	Integer ret
	UByte buf(8)
	
	ret = MODBUS_Read(&H0108, 4, ByRef buf())
	If ret <> NOERR Then GoTo ErrProc
	
	sts = ALARM_NONE
	
	' JA: モーターステータス
	' EN: Motor status
	If (buf(3) And &B01000000) > 0 Then
		sts = ALARM_OVERCURRENT
	ElseIf (buf(3) And &B10000000) > 0 Then
		sts = ALARM_OVERTEMP
	ElseIf (buf(3) And &B00000010) > 0 Then
		sts = ALARM_OVERVOLT
	ElseIf (buf(2) And &B00000010) > 0 Then
		sts = ALARM_UNDERVOLT
	ElseIf (buf(3) And &B00000001) > 0 Then
		 sts = ALARM_OVERFLOW

	' JA: 警告ステータス
	' EN: Warning status
	ElseIf (buf(7) And &B01000000) > 0 Then
		sts = WARNING_TEMP
	ElseIf (buf(7) And &B10000000) > 0 Then
		sts = WARNING_OVERLOAD
	ElseIf (buf(6) And &B00000010) > 0 Then
		sts = WARNING_WORKLOST
	ElseIf (buf(7) And &B00000001) > 0 Then
		sts = WARNING_GRIPFAIL
	EndIf
	SMC_LEHR_sts_ = sts
	
	' JA: サーボOFF/ON
	' EN: Servo OFF/ON
	If (buf(3) And &B00010000) = 0 Then
		SMC_LEHR_servo_onoff_ = 1
	Else
		SMC_LEHR_servo_onoff_ = 0
	EndIf
	
ErrProc:

	SMC_LEHR_sts_ = sts
	GetAllStatus = ret
	
Fend


'
' JA: pulse → 位置[mm]
' EN: pulse -> Position[mm]
'
Function PlsToPos(ps As UInt32) As Real

	If ps < &H80000000 Then
		' JA: 正の値
		' EN: Positive value
		PlsToPos = ps /300.0
	Else
		Int32 neg
		' JA: 負の値（2の補数）
		' EN: Negative value (2's complement)	
		neg = ((ps Xor &HFFFFFFFF) + 1) * -1
		PlsToPos = neg /300.0

	EndIf


Fend

'
' JA: 位置[mm] → pulse
' EN: Position[mm] -> pulse 
'
'
Function PosToPls(pos As Real) As UInt32

	PosToPls = pos * 300.0

Fend

'
' JA: pulse/sec →　速度[mm/sec] 　開：正の値、閉：負の値
' EN: pulse/sec ->　Speed[mm/sec] 　Open: Positive value, Close：Negative value
'
Function PlsToSpd(ps As UInt32) As Real

	If ps < &H80000000 Then
		' JA: 正の値
		' EN: Positive value
		PlsToSpd = ps /3.0
	Else
		Int32 neg
		' JA: 負の値（2の補数）
		' EN: Negative value (2's complement)		
		neg = ((ps Xor &HFFFFFFFF) + 1) * -1
		PlsToSpd = neg /3.0
	EndIf

Fend

'
' JA: 速度[mm/sec] → pulse/sec
' EN: Speed[mm/sec] -> pulse/sec
'
Function SpdToPls(spd As Real) As UInt32

	SpdToPls = spd * 3.0

Fend

'
' JA: % → トルク[N]　　開：正の値、閉：負の値
' EN: % -> Torque[N]	Open: Positive value, Close：Negative value
'
Function PerToTrq(per As UInt32) As Real
	
	If per < &H80000000 Then
		' JA: 正の値
		' EN: Positive value	
		PerToTrq = per * 16 / 5.0 - 1.25
	Else
		Int32 neg
		' JA: 負の値（2の補数）
		' EN: Negative value (2's complement)
		neg = ((per Xor &HFFFFFFFF) + 1) * -1
		PerToTrq = neg * 16 / 5.0 - 1.25
	EndIf
	
Fend

'
' JA: トルク[N] → %
' EN: Torque[N] -> %
'
Function TrqToPer(trq As Real) As UInt32

	TrqToPer = trq * 5.0 / 16.0 + 1.25

Fend


 ' ======================================================================
 '	Modbus TCP Functions
 ' ======================================================================
#include "Modbus.inc"
 ' ---------------------------------------------------------------------
 '	Global Variables
 ' ---------------------------------------------------------------------
 ' Debug flag
Global NonStatic Boolean Lib2692473628_G1
 ' ---------------------------------------------------------------------
 '	Functions
 ' ---------------------------------------------------------------------
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Print log for debugging
 '
 '	Args:
 '		message$  (String): message
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function DebugLog (message$ As String)
Function Lib2692473628_F8 (message$ As String)
	If Lib2692473628_G1 Then
		Print message$
	EndIf
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Clear TCP buffer.
 '
 '	Args:
 '		port  (Integer): TCP port
 '
 '	Returns:
 '		 (Integer): Modbus error
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function ClearNet (port As Integer) As Integer
Function Lib2692473628_F9 (port As Integer) As Integer
	OnErr GoTo ErrProc
	Integer dummy
	
	Do While ChkNet (port) > 0
		ReadBin #port, dummy
	Loop
	Lib2692473628_F9 = SUCCESS
	Exit Function
ErrProc:
	Lib2692473628_F9 = ERR_TCP_GENERAL
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Open TCP port.
 '
 '	Args:
 '		port  (Integer): TCP port
 '
 '	Returns:
 '		 (Integer): Error code if positive, ChkNet error if negative
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function ModbusTCP_Connect (port As Integer) As Integer
Function Lib2692473628_F10 (port As Integer) As Integer
	OnErr GoTo ErrProc
	Integer ret
	OpenNet #port As Client
	
	WaitNet #port, 3.0
	
	ret = ChkNet (port)
	If ret < 0 Then
		Lib2692473628_F10 = ret
		Exit Function
	EndIf
	Lib2692473628_F9 (port)
	
	Lib2692473628_F8 ("   ModbusTCP_Connect: port = " + Str$ (port))
	Lib2692473628_F10 = SUCCESS
	Exit Function
ErrProc:
	Lib2692473628_F8 ("   [ERROR]ModbusTCP_Connect: port = " + Str$ (port))
	Lib2692473628_F10 = ERR_TCP_GENERAL
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Close TCP port.
 '
 '	Args:
 '		port  (Integer): TCP port
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function ModbusTCP_Disconnect (port As Integer) As Integer
Function Lib2692473628_F11 (port As Integer) As Integer
	OnErr GoTo ErrProc
	
	Lib2692473628_F9 (port)
	CloseNet #port
	
	Lib2692473628_F8 ("   ModbusTCP_Disconnect: port = " + Str$ (port))
	Lib2692473628_F11 = SUCCESS
	Exit Function
	
ErrProc:
	Lib2692473628_F8 ("   [ERROR]ModbusTCP_Disconnect: port = " + Str$ (port))
	Lib2692473628_F11 = ERR_TCP_GENERAL
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Receive ADU  (Application Data Unit)
 '
 '	Args:
 '		buf  (ByRef Byte Array): Buffer to receive data
 '		bufLen  (Integer): Byte size of the buffer
 '		offset  (UByte): Offset to the data from PDU
 '		port  (Integer): TCP port
 '		timeout_sec  (Integer): Timeout seconds
 '
 '	Returns;
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_Receive (ByRef buf () As UByte, bufLen As Integer, offset As UByte, port As Integer, timeout_sec As Integer) As Integer
Function Lib2692473628_F12 (ByRef buf () As UByte, bufLen As Integer, offset As UByte, port As Integer, timeout_sec As Integer) As Integer
	OnErr GoTo ErrProc
	UByte bufHeader (7), data
	Integer index, total_tmoutcnt, tmoutcnt, errorcode, excode, length
	
	If ChkNet (port) <= -3 Then
		Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port) + " ChkNet = " + Str$ (ChkNet (port)))
		Lib2692473628_F12 = ERR_TCP_GENERAL
		Exit Function
	EndIf
	 'Read header 
	total_tmoutcnt = timeout_sec /0.05
	tmoutcnt = total_tmoutcnt
	Do While ChkNet (port) < 7 And tmoutcnt > 0
		If tmoutcnt < total_tmoutcnt Then
			Wait 0.05
		EndIf
		tmoutcnt = tmoutcnt - 1
	Loop
	If tmoutcnt <= 0 Then
		Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port) + " TIMEOUT HEADER")
		Lib2692473628_F12 = ERR_TCP_TIMEOUT
		Exit Function
	EndIf
	ReadBin #port, bufHeader (), 7
	
	length = bufHeader (4) * &H100 + bufHeader (5) - 1
	If length <= 2 Then
		ReadBin #port, errorcode
		ReadBin #port, excode
		
		Select excode
			Case 0
				Lib2692473628_F12 = ERR_MODBUS_PARAM_OUT_OF_RANGE
			Case 1
				Lib2692473628_F12 = ERR_MODBUS_INVALID_FUNCTION_CODE
			Case 2
				Lib2692473628_F12 = ERR_MODBUS_INVALID_ADDRESS
			Case 3
				Lib2692473628_F12 = ERR_MODBUS_INVALID_REGISTER
			Default
				Lib2692473628_F12 = ERR_MODBUS_RESPONSE
		Send
		Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port) + " EXCEPTION = " + Str$ (Lib2692473628_F12))
		Exit Function
	EndIf
	If length > bufLen Then
		Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port) + " TCP FORMAT")
		Lib2692473628_F12 = ERR_TCP_FORMAT
		Exit Function
	EndIf
	
	total_tmoutcnt = timeout_sec /0.05
	Do While ChkNet (port) < length And tmoutcnt > 0
		If tmoutcnt < total_tmoutcnt Then
			Wait 0.05
		EndIf
		tmoutcnt = tmoutcnt - 1
	Loop
	If tmoutcnt <= 0 Then
		Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port) + " TIMEOUT DATA")
		Lib2692473628_F12 = ERR_TCP_TIMEOUT
		Exit Function
	EndIf
	
	Do While offset > 0
		ReadBin #port, data
		offset = offset - 1
	Loop
	
	For Index = 0 To length - 1
		ReadBin #port, data
		buf (Index) = data
	Next
	Lib2692473628_F8 ("   Modbus_Receive: port = " + Str$ (port) + " data length = " + Str$ (length))
	Lib2692473628_F12 = SUCCESS
	Exit Function
	
ErrProc:
	Lib2692473628_F8 ("   [ERROR]Modbus_Receive: port = " + Str$ (port))
	Lib2692473628_F12 = ERR_TCP_GENERAL
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Create MBAP header
 '
 '	Args:
 '		buf  (ByRef UByte Array): Buffer
 '		length  (UByte): Byte size to send
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_CreateHeader (ByRef buf () As UByte, length As UByte)
Function Lib2692473628_F13 (ByRef buf () As UByte, length As UByte)
	
	 ' Transaction ID  (always 0)
	buf (0) = 0
	buf (1) = 0
	
	 ' Protocal ID  (always 0)
	buf (2) = 0
	buf (3) = 0
	
	 ' Field length  (after byte 6)
	buf (4) = 0
	buf (5) = length - 6
	
	 ' Unit identifier  (always 0)
	buf (6) = 0
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read single register
 '
 '	Args:
 '		value  (ByRef UShort): Variable to set register value
 '		port  (Integer): TCP port
 '		fcode  (UByte): Function code  (3: Holding register, 4: Input register)
 '		address  (UShort): Address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadRegister (ByRef value As UShort, port As Integer, fcode As UByte, address As UShort) As Integer
Function Lib2692473628_F14 (ByRef value As UShort, port As Integer, fcode As UByte, address As UShort) As Integer
	OnErr GoTo ErrProc
	
	Integer ret
	UByte bufSend (12), bufRecv (4)
 	
	 ' Function code
	bufSend (7) = fcode
	
	 ' Start address 
	bufSend (8) = address / &H100
	bufSend (9) = address And &HFF
	
	 ' Length
	bufSend (10) = 0
	bufSend (11) = 1
	
	 ' Header	
	Lib2692473628_F13 (ByRef bufSend (), 12)
	
	 ' Send data
	WriteBin #port, bufSend (), 12
	
	 ' Recieve data  (Timeout 3 sec)
	ret = Lib2692473628_F12 (ByRef bufRecv (), 4, 0, port, 3)
	If ret <> SUCCESS Then
		Lib2692473628_F14 = ret
		Exit Function
	EndIf
	
	value = bufRecv (2) * &H100 + bufRecv (3)
	
	Lib2692473628_F8 ("   Modbus_ReadRegister: port = " + Str$ (port) + " value = " + Hex$ (value) + " address = " + Hex$ (address) + " fcode = " + Hex$ (fcode))
	Lib2692473628_F14 = SUCCESS
	Exit Function
	
ErrProc:
	Lib2692473628_F8 ("   [ERROR]Modbus_ReadRegister: port = " + Str$ (port) + " address = " + Hex$ (address) + " fcode = " + Hex$ (fcode))
	Lib2692473628_F14 = ERR_TCP_GENERAL
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read single input register
 '
 '	Args:
 '		value  (ByRef UShort): Variable to set register value
 '		port  (Integer): TCP port
 '		address  (UShort): Address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadInputRegister (ByRef value As UShort, port As Integer, address As UShort) As Integer
Function Lib2692473628_F15 (ByRef value As UShort, port As Integer, address As UShort) As Integer
	Lib2692473628_F15 = Lib2692473628_F14 (ByRef value, port, 4, address)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read single holding register
 '
 '	Args:
 '		value  (ByRef UShort): Variable to set register value
 '		port  (Integer): TCP port
 '		address  (UShort): Address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadHoldingRegister (ByRef value As UShort, port As Integer, address As UShort) As Integer
Function Lib2692473628_F16 (ByRef value As UShort, port As Integer, address As UShort) As Integer
	Lib2692473628_F16 = Lib2692473628_F14 (ByRef value, port, 3, address)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read multiple registers
 '
 '	Args:
 '		values  (ByRef UShort Array): Register values
 '		count  (UShort): Number of registers to read
 '		port  (Integer): TCP port
 '		fcode  (UByte): Function code  (3: Holding register, 4: Input register)
 '		address  (UShort): Starting address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadRegisters (ByRef values () As UShort, count As UShort, port As Integer, fcode As UByte, address As UShort) As Integer
Function Lib2692473628_F17 (ByRef values () As UShort, count As UShort, port As Integer, fcode As UByte, address As UShort) As Integer
	
	OnErr GoTo ErrProc
	
	Integer ret
	Integer byteCount
	UByte bufSend (12), bufRecv (256)
	Integer index
 	
	 ' Function code
	bufSend (7) = fcode
	
	 ' Start address 
	bufSend (8) = address / &H100
	bufSend (9) = address And &HFF
	
	 ' Length
	bufSend (10) = count / &H100
	bufSend (11) = count And &HFF
	
	 ' Header	
	Lib2692473628_F13 (ByRef bufSend (), 12)
	
	 ' Send data
	WriteBin #port, bufSend (), 12
	
	 ' Recieve data  (Timeout 10sec)
	ret = Lib2692473628_F12 (ByRef bufRecv (), 2 + count * 2, 0, port, 10)
	If ret <> SUCCESS Then
		Lib2692473628_F17 = ret
		Exit Function
	EndIf
	For index = 0 To count - 1
		values (index) = bufRecv (2 + index * 2) * &H100 + bufRecv (3 + index * 2)
	Next
	
	Lib2692473628_F8 ("   Modbus_ReadRegisters: port = " + Str$ (port) + " count = " + Str$ (count) + " address = " + Hex$ (address) + " fcode = " + Hex$ (fcode))
	Lib2692473628_F17 = SUCCESS
	Exit Function
	
ErrProc:
	Lib2692473628_F8 ("   [ERROR]Modbus_ReadRegisters: port = " + Str$ (port) + " count = " + Str$ (count) + " address = " + Hex$ (address) + " fcode = " + Hex$ (fcode))
	Lib2692473628_F17 = ERR_TCP_GENERAL
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read multiple input registers
 '
 '	Args:
 '		values  (ByRef UShort Array): Register values
 '		count  (UShort): Number of registers to read
 '		port  (Integer): TCP port
 '		address  (UShort): Starting address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadInputRegisters (ByRef values () As UShort, count As UShort, port As Integer, address As UShort) As Integer
Function Lib2692473628_F18 (ByRef values () As UShort, count As UShort, port As Integer, address As UShort) As Integer
	Lib2692473628_F18 = Lib2692473628_F17 (ByRef values (), count, port, 4, address)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Read multiple holding registers
 '
 '	Args:
 '		values  (ByRef UShort Array): Register values
 '		count  (UShort): Number of registers to read
 '		port  (Integer): TCP port
 '		address  (UShort): Starting address
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_ReadHoldingRegisters (ByRef values () As UShort, count As UShort, port As Integer, address As UShort) As Integer
Function Lib2692473628_F19 (ByRef values () As UShort, count As UShort, port As Integer, address As UShort) As Integer
	Lib2692473628_F19 = Lib2692473628_F17 (ByRef values (), count, port, 3, address)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Write single register
 '
 '	Args:
 '		port  (Integer): TCP port
 '		address  (UShort): address
 '		value  (UShort): value
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Modbus_WriteRegister (port As Integer, address As UByte, value As UShort) As Integer
Function Lib2692473628_F20 (port As Integer, address As UByte, value As UShort) As Integer
	
	OnErr GoTo ErrProc
	
	UByte bufSend (12), bufRecv (5)
	Integer ret
	
	 ' Function code
	bufSend (7) = &H6
	
	 ' Start address 
	bufSend (8) = address / &H100
	bufSend (9) = address And &HFF
	
	 ' Data
	bufSend (10) =  (value / &H100)
	bufSend (11) =  (value And &HFF)
	
	 ' Header	
	Lib2692473628_F13 (ByRef bufSend (), 12)
	
	 ' Send data
	WriteBin #port, bufSend (), 12
	
	 ' Recieve data  (Timeout 3 sec)
	ret = Lib2692473628_F12 (ByRef bufRecv (), 5, 0, port, 3)
	If ret <> SUCCESS Then
		Lib2692473628_F8 ("   [ERROR]Modbus_WriteRegister: port = " + Str$ (port) + " value = " + Hex$ (value) + " address = " + Hex$ (address))
		Lib2692473628_F20 = ret
		Exit Function
	EndIf
	
	If bufRecv (0) <> bufSend (7) Or bufRecv (1) <> bufSend (8) Or bufRecv (2) <> bufSend (9) Then
		Lib2692473628_F8 ("   [ERROR]Modbus_WriteRegister: port = " + Str$ (port) + " value = " + Hex$ (value) + " address = " + Hex$ (address) + " MODBUS RESPONSE")
		Lib2692473628_F20 = ERR_MODBUS_RESPONSE
		Exit Function
	EndIf
	Lib2692473628_F8 ("   Modbus_WriteRegister: port = " + Str$ (port) + " value = " + Hex$ (value) + " address = " + Hex$ (address))
	Lib2692473628_F20 = SUCCESS
	Exit Function
	
ErrProc:
	Lib2692473628_F8 ("   [ERROR]Modbus_WriteRegister: port = " + Str$ (port) + " value = " + Hex$ (value) + " address = " + Hex$ (address))
	Lib2692473628_F20 = ERR_TCP_GENERAL
	
Fend

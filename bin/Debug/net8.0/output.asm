.386
.model flat, stdcall
.stack 4096
.data


.code

proc1 PROC
push ebp
mov ebp, esp
sub esp, 4
; local variable a at offset -4
mov eax, 1
	mov [ebp - 4], eax
	mov eax, [ebp - 4]
	
mov esp, ebp
pop ebp
ret
proc1 ENDP
proc2 PROC
push ebp
mov ebp, esp
sub esp, 4
; local variable a at offset -4
mov eax, 2
	mov [ebp - 4], eax
	mov eax, [ebp - 4]
	
mov esp, ebp
pop ebp
ret
proc2 ENDP
main PROC
push ebp
mov ebp, esp
sub esp, 60
; local variable a at offset -4
; local variable b at offset -8
; local variable c at offset -12
; local variable d at offset -16
; local variable e at offset -20
; local variable f at offset -24
; local variable g at offset -28
; local variable h at offset -32
; local variable i at offset -36
; local variable j at offset -40
; local variable k at offset -44
; local variable l at offset -48
; local variable m at offset -52
; local variable n at offset -56
; local variable finalResult at offset -60
mov eax, 10
	mov [ebp - 4], eax
	mov eax, 5
	mov [ebp - 8], eax
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	add eax, ebx
	mov [ebp - 12], eax
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	mov ecx, eax
	mov eax, ebx
	sub eax, ecx
	mov [ebp - 16], eax
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	mov ecx, eax
	mov eax, ebx
	imul eax, ecx
	mov [ebp - 20], eax
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	mov ecx, eax
	mov eax, ebx
	mov edx, 0
	idiv ecx
	mov [ebp - 24], eax
	; if statement
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	cmp ebx, eax
	setl al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_39_else
	; then block
	call proc1
		mov [ebp - 28], eax
		
	jmp ACB_IF_39_endif
	ACB_IF_39_else:
	; else block
	call proc2
		mov [ebp - 28], eax
		
	ACB_IF_39_endif:
	; if statement
	mov eax, [ebp - 4]
	push eax
	mov eax, [ebp - 8]
	pop ebx
	cmp ebx, eax
	setg al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_51_else
	; then block
	call proc1
		mov [ebp - 32], eax
		
	jmp ACB_IF_51_endif
	ACB_IF_51_else:
	; else block
	call proc2
		mov [ebp - 32], eax
		
	ACB_IF_51_endif:
	; if statement
	mov eax, [ebp - 4]
	push eax
	mov eax, 10
	pop ebx
	cmp ebx, eax
	setle al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_63_else
	; then block
	call proc1
		mov [ebp - 36], eax
		
	jmp ACB_IF_63_endif
	ACB_IF_63_else:
	; else block
	call proc2
		mov [ebp - 36], eax
		
	ACB_IF_63_endif:
	; if statement
	mov eax, [ebp - 8]
	push eax
	mov eax, 5
	pop ebx
	cmp ebx, eax
	setge al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_75_else
	; then block
	call proc1
		mov [ebp - 40], eax
		
	jmp ACB_IF_75_endif
	ACB_IF_75_else:
	; else block
	call proc2
		mov [ebp - 40], eax
		
	ACB_IF_75_endif:
	; if statement
	mov eax, [ebp - 4]
	push eax
	mov eax, 10
	pop ebx
	cmp ebx, eax
	sete al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_87_else
	; then block
	call proc1
		mov [ebp - 44], eax
		
	jmp ACB_IF_87_endif
	ACB_IF_87_else:
	; else block
	call proc2
		mov [ebp - 44], eax
		
	ACB_IF_87_endif:
	; if statement
	mov eax, [ebp - 4]
	push eax
	mov eax, 10
	pop ebx
	cmp ebx, eax
	setne al
	movzx eax, al
	cmp eax, 0
	je ACB_IF_99_else
	; then block
	call proc1
		mov [ebp - 48], eax
		
	jmp ACB_IF_99_endif
	ACB_IF_99_else:
	; else block
	call proc2
		mov [ebp - 48], eax
		
	ACB_IF_99_endif:
	mov eax, 1
	push eax
	mov eax, 0
	pop ebx
	or eax, ebx
	mov [ebp - 52], eax
	mov eax, 1
	push eax
	mov eax, 0
	pop ebx
	and eax, ebx
	mov [ebp - 56], eax
	mov eax, [ebp - 12]
	push eax
	mov eax, [ebp - 16]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 20]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 24]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 28]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 32]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 36]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 40]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 44]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 48]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 52]
	pop ebx
	add eax, ebx
	push eax
	mov eax, [ebp - 56]
	pop ebx
	add eax, ebx
	mov [ebp - 60], eax
	mov eax, [ebp - 60]
	
mov esp, ebp
pop ebp
ret
main ENDP
END main


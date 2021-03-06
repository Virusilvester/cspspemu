<?php

//function generate_test($opcode, $extra, $callback) {
//	$function_name = "test_{$opcode}{$extra}";
//	
//	echo "void $function_name";
//	
//	//echo "\n";
//	//echo ".globl {$function_name}\n";
//	//echo ".ent {$function_name}\n";
//	//echo "{$function_name}:\n";
//	//
//	//$callback();
//	//
//	//echo "jr \$ra\n";
//	//echo "nop\n";
//	//echo ".end {$function_name}\n";
//	//echo "\n";
//}

function to_uint16($value) {
	return ($value & 0xFFFF);
}

function to_sint16($value) {
	$value = to_uint16($value);
	if ($value & 0x8000) $value |= ~0xFFFF;
	return $value;
}

function generate_test_r2_i1($opcode, $signed, $values = NULL) {
	$min_sint16 = -pow(2, 15);
	$max_sint16 = pow(2, 15) - 1;

	$min_uint16 = 0;
	$max_uint16 = pow(2, 16) - 1;

	if ($values === NULL) {
		if ($signed) {
			$values = array(0, 1, 2, 3, 17, -1, -2, -3, -17, $min_sint16, $max_sint16, to_sint16(0xF721), to_sint16(0x368A));
		} else {
			$values = array(0, 1, 2, 3, 17, $max_sint16, 0xF721, 0x368A);
		}
	}

	//echo "__declspec(naked) ";
	//echo "naked ";
	foreach ($values as $k => $right) {
		echo "int test_{$opcode}_base_{$k}(int x) {";
		printf("int result; asm volatile(");
		echo "\"{$opcode} %0, %1, {$right}\"";
		//printf("\t\t\"jr \$ra\"\n", $opcode, $right);
		//printf("\t\t\"nop\"\n");
		printf(' : "=r"(result) : "r"(x));');
		printf(" return result;");
		echo " }\n";
	}

	echo "void test_{$opcode}_base(int x) {\n";
	{
		foreach (array_keys($values) as $k) {
			printf("\tprintf(\"%s,\", test_{$opcode}_base_%d(x));\n", '%d', $k);
		}
	}
	echo "}\n";

	echo "void test_{$opcode}() {\n";
	{
		foreach (array_values($values) as $left) {
			printf("\ttest_{$opcode}_base(%s);\n", $left);
		}
	}
	echo "}\n";
	echo "\n";
}

function generate_test_r3_i0($opcode, $signed, $values = NULL) {
	$min_sint16 = -pow(2, 15);
	$max_sint16 = pow(2, 15) - 1;

	$min_uint16 = 0;
	$max_uint16 = pow(2, 16) - 1;

	if ($values === NULL) {
		if ($signed) {
			$values = array(0, 1, 2, 3, 17, -1, -2, -3, -17, $min_sint16, $max_sint16, to_sint16(0xF721), to_sint16(0x368A));
		} else {
			$values = array(0, 1, 2, 3, 17, $max_sint16, 0xF721, 0x368A);
		}
	}

	//echo "__declspec(naked) ";
	//echo "naked ";
	echo "int test_{$opcode}_base_x(int x, int y) {";
	printf("int result; asm volatile(");
	echo "\"{$opcode} %0, %1, %2\"";
	//printf("\t\t\"jr \$ra\"\n", $opcode, $right);
	//printf("\t\t\"nop\"\n");
	printf(' : "=r"(result) : "r"(x), "r"(y));');
	printf(" return result;");
	echo " }\n";

	echo "void test_{$opcode}_base(int x) {\n";
	{
		foreach ($values as $k => $right) {
			printf("\tprintf(\"%s,\", test_{$opcode}_base_x(x, %d));\n", '%d', $right);
		}
	}
	echo "}\n";

	echo "void test_{$opcode}() {\n";
	{
		foreach (array_values($values) as $left) {
			printf("\ttest_{$opcode}_base(%s);\n", $left);
		}
	}
	echo "}\n";
	echo "\n";
}

$opcodes = array();

/*
ob_start();
{
	// EXTRA
	echo ".set noreorder\n";
	echo ".set noat\n";

	// TEXT
	echo ".text\n";
	echo ".align 4\n";

	{
		generate_test_r2_i1($opcodes[] = 'addi', $signed = true);
		generate_test_r2_i1($opcodes[] = 'addiu', $signed = false);
		generate_test_r2_i1($opcodes[] = 'andi', $signed = false);
		generate_test_r2_i1($opcodes[] = 'ori', $signed = false);
		generate_test_r2_i1($opcodes[] = 'xori', $signed = false);
		generate_test_r2_i1($opcodes[] = 'slti', $signed = true);
		generate_test_r2_i1($opcodes[] = 'sltiu', $signed = false);
	}

	// DATA
	echo ".section	.rodata\n";
	echo ".align 4\n";
	echo ".globl _printf_int_param\n";
	echo ".asciiz  \"%d,\"\n";
}
file_put_contents('alu_tests.S', ob_get_clean());

ob_start();
{
	foreach ($opcodes as $opcode) {
		printf("void test_%s();\n", $opcode);
	}
}
file_put_contents('alu_tests.h', ob_get_clean());
*/

ob_start();
{
	printf("// Autogenerated file. Please do not modify. To regenerate just execute 'php generate.php'\n");
	printf("#include <common.h>\n");
	printf("#include <pspkernel.h>\n");
	//printf("#include \"alu_tests.h\"\n");
	printf("\n");
	{
		// Arithmetic operations.
		generate_test_r3_i0($opcodes[] = 'add', $signed = true);
		generate_test_r3_i0($opcodes[] = 'addu', $signed = false);
		generate_test_r3_i0($opcodes[] = 'sub', $signed = true);
		generate_test_r3_i0($opcodes[] = 'subu', $signed = false);
		generate_test_r2_i1($opcodes[] = 'addi', $signed = true);
		generate_test_r2_i1($opcodes[] = 'addiu', $signed = false);
		
		// Logical Operations.
		generate_test_r3_i0($opcodes[] = 'and', $signed = false);
		generate_test_r3_i0($opcodes[] = 'or', $signed = false);
		generate_test_r3_i0($opcodes[] = 'xor', $signed = false);
		generate_test_r3_i0($opcodes[] = 'nor', $signed = false);
		generate_test_r2_i1($opcodes[] = 'andi', $signed = false);
		generate_test_r2_i1($opcodes[] = 'ori', $signed = false);
		generate_test_r2_i1($opcodes[] = 'xori', $signed = false);

		// Shift Left/Right Logical/Arithmethic (Variable).
		generate_test_r2_pos($opcodes[] = 'sll', $signed = false);
		generate_test_r2_pos($opcodes[] = 'sra', $signed = true);
		generate_test_r2_pos($opcodes[] = 'srl', $signed = false);
		generate_test_r2_pos($opcodes[] = 'rotr', $signed = false);
		generate_test_r3_i0($opcodes[] = 'sllv', $signed = false);
		generate_test_r3_i0($opcodes[] = 'srav', $signed = true);
		generate_test_r3_i0($opcodes[] = 'srlv', $signed = false);
		generate_test_r3_i0($opcodes[] = 'rotrv', $signed = false);

		// Set Less Than (Immediate) (Unsigned).
		generate_test_r3_i0($opcodes[] = 'slt', $signed = true);
		generate_test_r3_i0($opcodes[] = 'sltu', $signed = false);
		generate_test_r2_i1($opcodes[] = 'slti', $signed = true);
		generate_test_r2_i1($opcodes[] = 'sltiu', $signed = false);
		
		// LUI
		//generate_test_r1_i1($opcodes[] = 'lui', $signed = false);
		
		// Sign Extend Byte/Half word.
		generate_test_r2_i0($opcodes[] = 'seb', $signed = false);
		generate_test_r2_i0($opcodes[] = 'seh', $signed = false);
		
		// BIT REVerse.
		generate_test_r2_i0($opcodes[] = 'bitrev', $signed = false);
		
		// MAXimum/MINimum.
		generate_test_r3_i0($opcodes[] = 'max', $signed = false);
		generate_test_r3_i0($opcodes[] = 'min', $signed = false);
		
		generate_test_hilo_r2($opcodes[] = 'div', $signed = true);
		generate_test_hilo_r2($opcodes[] = 'divu', $signed = false);

		generate_test_hilo_r2($opcodes[] = 'mult', $signed = true);
		generate_test_hilo_r2($opcodes[] = 'multu', $signed = false);
		generate_test_hilo_r2($opcodes[] = 'madd', $signed = true);
		generate_test_hilo_r2($opcodes[] = 'maddu', $signed = false);
		generate_test_hilo_r2($opcodes[] = 'msub', $signed = true);
		generate_test_hilo_r2($opcodes[] = 'msubu', $signed = false);
	}
	printf("int main(int argc, char *argv[]) {\n");
	foreach ($opcodes as $opcode) {
		printf("\tprintf(\"%s: \"); test_%s(); printf(\"\\n\");\n", $opcode, $opcode);
	}
	printf("\n");
	printf("\treturn 0;\n");
	printf("}\n");
}
file_put_contents('cpu_alu.c', ob_get_clean());

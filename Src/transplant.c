#include "transplant.h"

#define ITM_Port8(n)    (*((volatile unsigned char *)(0xE0000000+4*n)))
#define ITM_Port16(n)   (*((volatile unsigned short*)(0xE0000000+4*n)))
#define ITM_Port32(n)   (*((volatile unsigned long *)(0xE0000000+4*n)))

#define DEMCR           (*((volatile unsigned long *)(0xE000EDFC)))
#define TRCENA          0x01000000

struct __FILE { int handle; /* Add whatever you need here */ };
FILE __stdout;
FILE __stdin;

/*
int fputc(int ch, FILE *f) {
  if (DEMCR & TRCENA) {
    while (ITM_Port32(0) == 0);
    ITM_Port8(0) = ch;
  }
  return(ch);
}
*/

uint32_t flash_sector_address( int16_t index ){
    
    if( index > SECTOR_TOTAL_NUM || index < 1 ){
        printf( "Fan area index error\r\n" );
        while( true );
    }
    
    uint32_t realaddr = FLASH_BASE;
    
    #if defined CORTEX_M3
        realaddr += ( index ) * KEY_VALUE_SIZE;
    #else
        //stm32f407ve       16 16 16 16 64 128 128 128
        if( index > 0 ){
            realaddr += ( index > 3 ? 4 : index ) * 16 * 1024;
        }
        if( index > 4 ){
            realaddr += 1 * 64 * 1024;
        }
        if( index > 5 ){
            realaddr += ( index - 5 ) * 128 * 1024;
        }
    #endif
    
    return realaddr;
}

bool flash_legal_sector_address( int32_t flashaddr ){
    for( uint16_t i = 1; i < SECTOR_TOTAL_NUM; i++ ){
        if( flashaddr == flash_sector_address( i ) ){
            return true;
        }
    }
    return false;
}

int16_t flash_sector_index( uint32_t flashaddr ){
    for( int16_t i = 1; i < SECTOR_TOTAL_NUM; i++ ){
        if( flashaddr == flash_sector_address( i ) ){
            return i;
        }
    }
    return -1;
}






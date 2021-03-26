#ifndef __TRANSPLANT__H
#define __TRANSPLANT__H
#include <stdbool.h>
#include <stdio.h>

#define SYS  false//true

#define CORTEX_M4

#if defined CORTEX_M3
    //#define _STM32L_
    #if defined _STM32L_
        #include "stm32l1xx_hal.h"
    #else
        #include "stm32f1xx_hal.h"
    #endif
#else
    #include "stm32f4xx_hal.h"
#endif

#define STRINGS_HEAD_FLAG   0xef1234ef
#define UINT32_INIT_FLAG    0x1024
#define STRINGS_INIT_FLAG   "OK"

#if defined _STM32L_
    #define BACKUP_FLAG_CLEAR_FLAG  0x0000ffff
    #define ERASURE_STATE           0x00000000
    #define FILL_STATE              ~ERASURE_STATE
#else
    #define BACKUP_FLAG_CLEAR_FLAG  0x00000000
    #define ERASURE_STATE           0xffffffff
    #define FILL_STATE              ~ERASURE_STATE
#endif

#define SECTOR_NUM              1
#define SECTOR_TOTAL_NUM        8
#define KEY_VALUE_SIZE          ( 16*1024 )

#define FLASH_MAX_SIZE          ( 512 * 1024 )
#define FLASH_END_ADDR          ( FLASH_BASE + FLASH_MAX_SIZE )

uint32_t flash_sector_address( int16_t index );
#define  ADDRESS_MAPPING(X)     flash_sector_address( X )

uint32_t flash_sector_address( int16_t index );
bool flash_legal_sector_address( int32_t flashaddr );
int16_t flash_sector_index( uint32_t flashaddr );

#endif


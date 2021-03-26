#ifndef __INSID__FLASH__H__
#define __INSID__FLASH__H__
#include <stdint.h>
#include <stdbool.h>
#include "transplant.h"

bool flash_write( const uint8_t *ramaddr, uint32_t flashaddr, int32_t size );
bool flash_read( int32_t flashAddr , uint8_t *ramBuffer , uint16_t bytesLen );
bool flash_erase( int32_t flashaddr, uint32_t page );

#endif


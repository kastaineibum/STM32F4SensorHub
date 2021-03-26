# STM32F4SensorHub
基于STM32F407的SensorHub，集成了各种常见传感器和FreeRTOS，从STM32CubeMX构建扩充。

## 气压温度传感器
bm280.c
bm280.h
bmp280.c
bmp280.h
bsp_driver_sd.c
## 国产温度传感器
dht11.c
dht11.h
dmpKey.h
dmpmap.h
dwt_stm32_delay.c
## exFAT文件系统
fatfs.c
fatfs_platform.c
## 调度器
freertos.c
## 片上闪存存储字符串参数
insideflash.c
insideflash.h
## 加速度计
inv_mpu.c
inv_mpu.h
inv_mpu_dmp_motion_driver.c
inv_mpu_dmp_motion_driver.h
key_value.c
key_value.h
## 兼容性I2C总线驱动
llgpioiic.c
llgpioiic.h
main.c
## MODBUS-RS485驱动
modbus.c
modbus.h
## 使用ST驱动器的步进电机驱动
motor.c
motor.h
## 加速度计与地磁传感器
mpu9250.c
mpu9250.h
## 500万像素豪威摄像头驱动
ov5640.c
sccb.c
## SD卡驱动
sd_diskio.c
stm32f4xx_hal_msp.c
stm32f4xx_hal_timebase_tim.c
stm32f4xx_it.c
system_stm32f4xx.c
transplant.c
transplant.h
## USB驱动
usb_device.c
usbd_conf.c
usbd_desc.c
usbd_storage_if.c
## 激光距离传感器驱动
vl53l1_platform.c 

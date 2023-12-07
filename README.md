# STM32F4SensorHub
因更多的使用其他系列的MCU，本项目已于2022年终止维护。
基于STM32F407的SensorHub，集成了各种常见传感器和FreeRTOS，从STM32CubeMX构建扩充。同时提供完全开源的原理图和PCB板图，可联系栗子树科技 http://3mn.net

## 博世气压温度传感器BMP280
bm280.c
bm280.h
bmp280.c
bmp280.h
## 奥松温度传感器DHT11
dht11.c
dht11.h
## 文件系统exFAT
fatfs.c
fatfs_platform.c
## 调度器FreeRTOS
freertos.c
## 片上闪存存储字符串和数字参数
insideflash.c
insideflash.h
transplant.c
transplant.h
key_value.c
key_value.h
## 兼容性I2C总线驱动
llgpioiic.c
llgpioiic.h
## RS485上的MODBUS驱动
modbus.c
modbus.h
## 使用ST驱动器LM298的步进电机
motor.c
motor.h
## 加速度计与地磁传感器MPU9250
dmpKey.h
dmpmap.h
inv_mpu.c
inv_mpu.h
inv_mpu_dmp_motion_driver.c
inv_mpu_dmp_motion_driver.h
mpu9250.c
mpu9250.h
## 500万像素豪威摄像头OV5640
ov5640.c
sccb.c
## 高速SD卡
bsp_driver_sd.c
sd_diskio.c
## USB存储外设
usb_device.c
usbd_conf.c
usbd_desc.c
usbd_storage_if.c
## 激光距离传感器VL53L1X
vl53l1_platform.c 

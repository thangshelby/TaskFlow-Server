import boto3
from PIL import Image
from io import BytesIO
from aws_xray_sdk.core import patch_all
patch_all()

s3Client = boto3.client('s3')

def resize_image(src_bucket_name, src_key, dst_bucket_name, dst_key, width, height):
    in_mem_file = BytesIO()
    
    file_byte_string= s3Client.get_object(Bucket=src_bucket_name, Key=src_key)['Body'].read()
    image = Image.open(BytesIO(file_byte_string))
    resized_image = image.resize((width, height))

    resized_image.save(in_mem_file, image.format)
    in_mem_file.seek(0)
    s3Client.put_object(Bucket=dst_bucket_name, Key=dst_key, Body=in_mem_file)

    return {
        'statusCode': 200,
        'body': 'Image resized successfully'
    }

def lambda_handler(event, context):
    
    width = 100
    height = 100
    
    src_bucket_name = ''
    obj_key = ''
    for obj in event['Records']:
        src_bucket_name = obj['s3']['bucket']['name']
        obj_key = obj['s3']['object']['key']
        
        dst_key = obj_key.replace('raw','processed')
        resize_image(src_bucket_name=src_bucket_name, src_key=obj_key, dst_bucket_name=src_bucket_name, dst_key= dst_key, width=width, height=height)
    
    print('Function executed successfully')
    return {
        'statusCode': 200,
        'body': 'Function executed successfully'        
    }
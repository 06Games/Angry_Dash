//(c) Brian Chasalow 2014 - brian@chasalow.com
// Edits by Miha Krajnc
#import <GLKit/GLKit.h>
 
extern "C" {
 
  typedef void (*TextureLoadedCallback)(int texID, int originalUUID, int w, int h);
  static TextureLoadedCallback textureLoaded;
  static GLKTextureLoader* asyncLoader = nil;
 
  void RegisterFastTexture2DCallbacks(void (*cb)(int texID, int originalUUID, int w, int h)){
      textureLoaded = *cb;
  }
 
  void CreateFastTexture2DFromAssetPath(const char* assetPath, int uuid, bool resize, int resizeW, int resizeH){
      @autoreleasepool {
          NSDictionary* options = [NSDictionary dictionaryWithObjectsAndKeys:
                                   [NSNumber numberWithBool:YES],
                                   GLKTextureLoaderOriginBottomLeft,
                                   nil];
 
          NSString* assetPathString = [NSString stringWithCString: assetPath encoding:NSUTF8StringEncoding];
 
          if(asyncLoader == nil) {
              asyncLoader = [[GLKTextureLoader alloc] initWithSharegroup:[[EAGLContext currentContext] sharegroup]];
          }
 
          if(resize){
              // UIImage* img = [UIImage imageWithContentsOfFile:assetPathString];
              // __block UIImage* smallerImg = [img resizedImage:CGSizeMake(resizeW, resizeH) interpolationQuality:kCGInterpolationDefault ];
              //
              // [asyncLoader textureWithCGImage:[smallerImg CGImage]
              //                         options:options
              //                           queue:NULL
              //               completionHandler:^(GLKTextureInfo *textureInfo, NSError *outError) {
              //                   if(outError){
              //                     smallerImg = nil;
              //                     NSLog(@&quot;got error creating texture at path: %@. error: %@ &quot;, assetPathString,[outError localizedDescription] );
              //                       textureLoaded(-1, uuid, 0, 0);
              //                   }
              //                   else{
              //                       textureLoaded(textureInfo.name, uuid, resizeW, resizeH);
              //                   }
              //               }];
 
          } else {
              [asyncLoader textureWithContentsOfFile:assetPathString
                           options:options
                           queue:NULL
                           completionHandler:^(GLKTextureInfo *textureInfo, NSError *outError) {
                    if(outError){
                        NSLog(@&quot;got error creating texture at path: %@. error: %@ &quot;, assetPathString,[outError localizedDescription] );
                        NSLog(@&quot;returning texID -1 &quot;);
                        textureLoaded(-1, uuid, 0, 0);
                    }
                    else
                    {
                      //this will get returned on the main thread cuz the queue above is NULL
                      textureLoaded(textureInfo.name, uuid, textureInfo.width, textureInfo.height);
                    }
                }];
          }
      }
  }
 
  void DeleteFastTexture2DAtTextureID(int texID){
      @autoreleasepool {
          GLuint texIDGL = (GLuint)texID;
          if(texIDGL > 0){
              if(glIsTexture(texIDGL)){
                  NSLog(@&quot;deleting a texture because it's a texture. %i&quot;, texIDGL);
                  glDeleteTextures(1, &amp;texIDGL);
              }
          }
      }
  }
}
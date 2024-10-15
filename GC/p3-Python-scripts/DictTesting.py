gestures = ['Forward','Backward']

bufferDict = {}
for gesture in gestures:
    bufferDict[gesture] = 0
bufferDict['No gesture'] = 0

#print(max(bufferDict.values()))

print(bufferDict)
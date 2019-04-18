# StreamIndexingUtils
StreamIndexingUtils project is a set of classes that allow packing multiple files into one file-container.

Consists of:
* ContentIndex - stores data about each file's position.
* IndexedWriteStream - stream to write new file into the container.
* IndexedReadOnlyStream - stream to read a file from the container.
* IndexSerializer - writes/reads ContentIndex to/from the container.
* IndexedStreamReaderWriter - facade-like class that utilizes classes above to provide their functionality in easier, but less flexible manner.

StreamIndexingUtils.Demo - is a WinForms demo application, written without much effort, that shows how to use the functionality of IndexedStreamReaderWriter class.

## License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/wolf8196/Mercury/blob/master/LICENSE) file for details

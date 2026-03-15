import sys

def undo_rewrite(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Remove the inserted headers
    header_block = '#if PORTING\nthrow new NotImplementedException("Unported Controller method");\n#else //!PORTING\n'
    content = content.replace(header_block, '')
    
    # Remove the inserted footers
    # Note that we inserted \n#endif\n
    footer_block = '\n#endif\n'
    content = content.replace(footer_block, '')
    
    # Also we might have inserted just `#endif` somewhere
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        
    print("Undo complete")

if __name__ == '__main__':
    undo_rewrite(sys.argv[1])
